using Dapper;
using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using System.Collections.Generic;
using System.Data.SQLite;
using LhDev.ShoppingLister.ExtensionMethods;

namespace LhDev.ShoppingLister.Services;

public interface IDbItemService
{
    Task<Item> AddItemAsync(NewItemParameters userParams);
    Task<Item> AddItemAsync(string name, string username, string listName);
    Task<int> GetItemCountAsync();
    Task<Item> GetItemAsync(int id);
    Task<Item[]> GetItemsAsync(int offset, int count);
    Task<Item[]> GetItemsByListIdAsync(int listId);
    Task<Item> UpdateItemAsync(int itemId, int userId, string newName);
    Task DeleteItemAsync(int itemId, int userId);
}

public class DbItemService : IDbItemService
{
    public static Item AddItem(string name, string username, string listName)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        return AddItem(name, username, listName, dbConn);
    }

    public static Item AddItem(string name, string username, string listName, SQLiteConnection dbConn)
    {
        // Check if a user by this username exists.
        var user = DbUserService.GetUser(dbConn, "username = @username", new { username = username.ToLower() })
                   ?? throw ShoppingListerWebException.ApiUsernameNotFound(username);

        // Check if a user by this username exists.
        var list = dbConn.GetItem<List>("UserId = @userId AND Name = @listName", new { userId = user.Id, listName })
                   ?? throw ShoppingListerWebException.ApiListNotFound(username);

        return Create(dbConn, list.Id, name);
    }

    public Task<Item> AddItemAsync(NewItemParameters userParams) 
        => AddItemAsync(userParams.Name, userParams.Username, userParams.ListName);

    public Task<Item> AddItemAsync(string name, string username, string listName)
    {
        var task = new Task<Item>(() => AddItem(name, username, listName));
        task.Start();

        return task;
    }

    public Task<int> GetItemCountAsync()
    {
        var task = new Task<int>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var count = dbConn.ExecuteScalar("SELECT COUNT(Id) FROM Item");

            return Convert.ToInt32(count);
        });
        task.Start();

        return task;
    }

    public async Task<Item> GetItemAsync(int id)
    {
        await using var dbConn = DbManager.CreateDbConnection();
        await dbConn.OpenAsync();
        
        return await dbConn.GetItemAsync<Item>("Id = @id", new { id }) ?? throw ShoppingListerWebException.ApiItemIdNotFound(id);
    }

    public Task<Item[]> GetItemsAsync(int offset, int count)
    {
        var task = new Task<Item[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var parameters = new { offset, count };

            return dbConn.Query<Item>("SELECT * FROM Item LIMIT @count OFFSET @offset", parameters).ToArray();
        });
        task.Start();

        return task;
    }

    public Task<Item[]> GetItemsByListIdAsync(int listId) =>
        Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return GetItems(dbConn, listId);
        });

    public Task<Item> UpdateItemAsync(int itemId, int userId, string newName) =>
        Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            // Check if a user by this username exists.
            var user = DbUserService.GetUser(dbConn, "Id = @userId", new { userId })
                       ?? throw ShoppingListerWebException.ApiUserIdNotFound(userId);

            // Check if the item exists
            var item = dbConn.GetItem<Item>("Id = @itemId", new { itemId })
                       ?? throw ShoppingListerWebException.ApiItemIdNotFound(itemId);

            // Get the list
            var list = dbConn.GetItem<List>("Id = @listId", new { listId = item.ListId })
                       ?? throw ShoppingListerWebException.ApiListIdNotFound(item.ListId);

            if (user.Id != list.UserId)
                throw ShoppingListerWebException.NotAuthorised("You are not authorised to update this item.");

            if (ExistsByName(dbConn, list.Id, newName)) throw ShoppingListerWebException.ItemExists(newName, list.Id);

            dbConn.Execute("UPDATE Item SET Name = @newName WHERE Id = @itemId", new { newName, itemId });
            item.Name = newName;

            return item;
        });

    public Task DeleteItemAsync(int itemId, int userId) =>
        Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            // Check if a user by this username exists.
            var user = DbUserService.GetUser(dbConn, "Id = @userId", new { userId })
                       ?? throw ShoppingListerWebException.ApiUserIdNotFound(userId);

            // Check if the item exists
            var item = dbConn.GetItem<Item>("Id = @itemId", new { itemId })
                       ?? throw ShoppingListerWebException.ApiItemIdNotFound(itemId);

            // Get the list
            var list = dbConn.GetItem<List>("Id = @listId", new { listId = item.ListId })
                       ?? throw ShoppingListerWebException.ApiListIdNotFound(item.ListId);

            if (user.Id != list.UserId)
                throw ShoppingListerWebException.NotAuthorised("You are not authorised to delete this item.");

            // Get the list
            if (dbConn.Exists<WorkingListItem>("ListId = @listId AND ItemId = @itemId", new { itemId, listId = item.ListId }))
                throw ShoppingListerWebException.CantDeleteItemInWorkingList();

            dbConn.Execute("DELETE FROM Item WHERE Id = @itemId", new { itemId });
        });







    public static Item[] GetItems(SQLiteConnection dbConn, int listId) => dbConn.GetItems<Item>("ListId = @listId", new { listId });
    public static Task<Item[]> GetItemsAsync(SQLiteConnection dbConn, int listId) 
        => dbConn.GetItemsAsync<Item>("ListId = @listId", new { listId });

    public static bool Exists(SQLiteConnection dbConn, int itemId) => dbConn.Exists<Item>("Id = @itemId", new { itemId });
    public static Task<bool> ExistsAsync(SQLiteConnection dbConn, int itemId) 
        => dbConn.ExistsAsync<Item>("Id = @itemId", new { itemId });
    public static bool ExistsByName(SQLiteConnection dbConn, int listId, string name) 
        => dbConn.Exists<Item>("ListId = @listId AND Name = @name", new { listId, name });
    public static Task<bool> ExistsByNameAsync(SQLiteConnection dbConn, int listId, string name) 
        => dbConn.ExistsAsync<Item>("ListId = @listId AND Name = @name", new { listId, name });


    public static Task<Item> CreateAsync(SQLiteConnection dbConn, int listId, string name) 
        => Task.Run(() => Create(dbConn, listId, name));

    public static Item Create(SQLiteConnection dbConn, int listId, string name)
    {
        // Trim the whitespace from the name, Android devices usually leave a space at the end of the text box...
        name = name.Trim();

        var @params = new { name, listId };
        // Check if an item with this name and listId already exists.
        if (ExistsByName(dbConn, listId, name)) throw ShoppingListerWebException.ItemExists(name, listId);

        // Create a new Item record.
        var def = new CommandDefinition("INSERT INTO Item (ListId, Name) VALUES (@listId, @name)", @params);
        if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateItem();

        // Get List object.
        var itemId = dbConn.LastInsertRowId;
        return dbConn.GetItem<Item>("Id = @id", new { id = itemId })
               ?? throw ShoppingListerWebException.CouldNotCreateItem();
    }
}