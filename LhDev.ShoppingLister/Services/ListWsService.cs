using LhDev.ShoppingLister.DbModels;
using System.Text.Json;
using LhDev.ShoppingLister.ExtensionMethods;
using System.Net.WebSockets;
using LhDev.ShoppingLister.Exceptions;
using System.Text;
using System.Data.SQLite;
using LhDev.ShoppingLister.ApiModels;

namespace LhDev.ShoppingLister.Services;

public interface IListWsService : IWsService
{
    //Task ConnectedAsync(WebSocketManager contextWebSockets, int listId);
}

public class ListWsService : WsService<int>, IListWsService
{
    public override async Task<string?> ProcessCommand((int, User, WebSocket) triple, string command, string? data)
    {
        var (listId, user, _) = triple;
        
        await using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();
        
        var obj = command switch
        {
            "getItems" => await DbItemService.GetItemsAsync(dbConn, listId),
            "getWorkingItems" => await DbWorkingListService.GetItemsAsync(dbConn, listId),
            "setTicked" => await SetTickedAsync(dbConn, triple, data),
            "addWorkingItem" => await AddWorkingItemAsync(dbConn, triple, data),
            "createNewItem" => await CreateNewItemAsync(dbConn, triple, data),
            "cleanList" => await CleanListAsync(dbConn, triple, data),
            _ => throw new Exception($"Command '{command}' not implemented")
        };

        return obj == null ? null : $"{command}|{JsonSerializer.Serialize(obj)}";
    }

    private async Task<object?> SetTickedAsync(SQLiteConnection dbConn, (int, User, WebSocket) triple, string? data)
    {
        if (string.IsNullOrEmpty(data)) throw new Exception("No data mate");

        var @params = JsonSerializer.Deserialize<SetTicked>(data);
        if (@params == null) throw new Exception("Bad data mate");

        var (listId, _, _) = triple;
        if (await DbWorkingListService.GetItemAsync(dbConn, @params.ItemId) == null) throw new Exception("Not this again.");
        
        await DbWorkingListService.SetTickedAsync(dbConn, listId, @params.ItemId, @params.Ticked);
        await SendDataToOtherSocketsAsync(triple, Encoding.UTF8.GetBytes($"setTicked|{data}"), (thisId, otherId) => thisId != otherId);

        return null;
    }

    private async Task<object?> AddWorkingItemAsync(SQLiteConnection dbConn, (int, User, WebSocket) triple, string? data)
    {
        if (string.IsNullOrEmpty(data)) throw new Exception("No data mate");
        if (!int.TryParse(data, out var itemId)) throw new Exception("Bad data mate");

        var (listId, _, _) = triple;
        if (await DbWorkingListService.GetItemAsync(dbConn, itemId) != null) return null;

        var item = await DbWorkingListService.AddItemToWorkingListAsync(dbConn, listId, itemId);
        var retData = $"addWorkingItem|{JsonSerializer.Serialize(item)}";
        
        await SendDataToOtherSocketsAsync(triple, Encoding.UTF8.GetBytes(retData), OnlyThisList);

        return item;
    }

    private static bool OnlyThisList(int thisListId, int otherListId) => thisListId != otherListId;

    private async Task<object?> CreateNewItemAsync(SQLiteConnection dbConn, (int, User, WebSocket) triple, string? data)
    {
        if (string.IsNullOrEmpty(data)) throw new Exception("No data mate");
        var (listId, _, _) = triple;

        if (await DbItemService.ExistsByNameAsync(dbConn, listId, data)) return null;

        var item = await DbItemService.CreateAsync(dbConn, listId, data);
        var workingItem = await DbWorkingListService.AddItemToWorkingListAsync(dbConn, listId, item.Id);

        var newItems = new
        {
            Item = item,
            WorkingItem = workingItem,
        };
        var retData = $"createNewItem|{JsonSerializer.Serialize(newItems)}";
        await SendDataToOtherSocketsAsync(triple, Encoding.UTF8.GetBytes(retData), OnlyThisList);

        return newItems;
    }

    private async Task<object?> CleanListAsync(SQLiteConnection dbConn, (int, User, WebSocket) triple, string? data)
    {
        var (listId, _, _) = triple;

        await DbWorkingListService.DeleteCheckedAsync(dbConn, listId); 
        var workingItems = await DbWorkingListService.GetItemsAsync(dbConn, listId);

        await SendDataToOtherSocketsAsync(triple, 
            Encoding.UTF8.GetBytes($"cleanList|{JsonSerializer.Serialize(workingItems)}"), OnlyThisList);

        return workingItems;
    }
}