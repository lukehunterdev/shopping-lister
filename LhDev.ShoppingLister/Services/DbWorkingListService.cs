using Dapper;
using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using System.Data.SQLite;
using System.Xml.Linq;
using LhDev.ShoppingLister.ExtensionMethods;

namespace LhDev.ShoppingLister.Services;

public interface IDbWorkingListService
{
    Task<WorkingListItem> AddItemToListAsync(int listId, int itemId, bool ticked = false);
    Task<WorkingItem[]> GetItemsByListIdAsync(int listId);
}

public class DbWorkingListService : IDbWorkingListService
{
    public static WorkingListItem AddItemToWorkingList(int listId, int itemId, bool ticked = false)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        return AddItemToWorkingList(dbConn, listId, itemId, ticked);
    }

    public static WorkingListItem AddItemToWorkingList(SQLiteConnection dbConn, int listId, int itemId, bool ticked = false)
    {
        if (!DbListService.Exists(dbConn, listId))
            throw ShoppingListerWebException.ApiListIdNotFound(listId);
        if (!DbItemService.Exists(dbConn, itemId))
            throw ShoppingListerWebException.ApiItemIdNotFound(itemId);
        if (Exists(dbConn, itemId))
            throw ShoppingListerWebException.ApiItemIdNotFound(itemId);

        var orderNum = GetMaxOrdinal(dbConn, listId) + 1;

        // Create a new WorkingListItem record.
        var def = new CommandDefinition("INSERT INTO WorkingListItem (ListId, ItemId, Ordinal, Ticked) VALUES " +
                                        "(@listId, @itemId, @orderNum, @ticked)", 
                                        new { listId, itemId, orderNum, ticked } );
        if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateItem();

        dbConn.Execute("UPDATE Item SET Usage = Usage + 1 WHERE Id = @itemId", new { itemId });

        return dbConn.GetItem<WorkingListItem>("ListId = @listId AND ItemId = @itemId", new { listId, itemId }) 
               ?? throw ShoppingListerWebException.CouldNotCreateItem();

    }

    public static Task<WorkingListItem> AddItemToWorkingListAsync(SQLiteConnection dbConn, int listId, int itemId, bool ticked = false)
        => Task.Run(() => AddItemToWorkingList(dbConn, listId, itemId, ticked));

    public Task<WorkingListItem> AddItemToListAsync(int listId, int itemId, bool ticked = false) 
        => Task.Run(() => AddItemToWorkingList(listId, itemId, ticked));

    public Task<WorkingItem[]> GetItemsByListIdAsync(int listId)
    {
        var task = new Task<WorkingItem[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return dbConn.Query<WorkingItem>("SELECT ItemId AS Id, Name, Ticked " +
                                             "FROM WorkingListItem INNER JOIN Item ON WorkingListItem.ItemId = Item.Id " +
                                             "WHERE WorkingListItem.ListId = @listId ORDER BY Ordinal ASC",
                                             new {listId}).ToArray();
        });
        task.Start();

        return task;
    }


    private static CommandDefinition SetTickedDef(int listId, int itemId, bool ticked)
        => new("UPDATE WorkingListItem SET Ticked = @ticked WHERE ListId = @listId AND ItemId = @itemId", 
               new { listId, itemId, ticked });

    public static void SetTicked(SQLiteConnection dbConn, int listId, int itemId, bool ticked) 
        => dbConn.Execute(SetTickedDef(listId, itemId, ticked));
    
    public static async Task SetTickedAsync(SQLiteConnection dbConn, int listId, int itemId, bool ticked) 
        => await dbConn.ExecuteAsync(SetTickedDef(listId, itemId, ticked));


    public static WorkingListItem? GetItem(SQLiteConnection dbConn, int itemId)
        => dbConn.GetItem<WorkingListItem>("ItemId = @itemId", new { itemId });
    public static Task<WorkingListItem?> GetItemAsync(SQLiteConnection dbConn, int itemId)
        => dbConn.GetItemAsync<WorkingListItem>("ItemId = @itemId", new { itemId });

    public static WorkingListItem[] GetItems(SQLiteConnection dbConn, int listId)
        => dbConn.GetItems<WorkingListItem>("ListId = @listId", new { listId });
    public static Task<WorkingListItem[]> GetItemsAsync(SQLiteConnection dbConn, int listId)
        => dbConn.GetItemsAsync<WorkingListItem>("ListId = @listId", new { listId });

    public static bool Exists(SQLiteConnection dbConn, int itemId)
        => dbConn.Exists<WorkingListItem>("ItemId = @itemId", new { itemId });

    public static long GetMaxOrdinal(SQLiteConnection dbConn, int listId)
    {
        var def = new CommandDefinition($"SELECT MAX(Ordinal) FROM WorkingListItem " +
                                        $"WHERE ListId = @listId", new { listId });

        return dbConn.ExecuteScalar<long>(def);
    }


    public static Task DeleteCheckedAsync(SQLiteConnection dbConn, int listId) =>
        dbConn.ExecuteAsync(new CommandDefinition($"DELETE FROM WorkingListItem WHERE Ticked = TRUE AND ListId = @listId", 
            new { listId }));
}