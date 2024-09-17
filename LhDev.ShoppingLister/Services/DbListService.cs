using Dapper;
using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using System.Data.SQLite;
using LhDev.ShoppingLister.ExtensionMethods;

namespace LhDev.ShoppingLister.Services;

public interface IDbListService
{
    Task<List> AddListAsync(NewListParameters userParams);
    Task<List> AddListAsync(string name, string username);
    Task<int> GetListCountAsync();
    Task<List> GetListAsync(int id);
    Task<List[]> GetListsAsync(int offset, int count);
    Task<List[]> GetListsByUserIdAsync(int userId);
    Task<List[]> GetSharedListsByUserIdAsync(int userId);
    Task<bool> UserHasAccessAsync(int listId, int userId);
    Task ShareListWithUserAsync(int listId, int userId);
    Task ShareListWithUserAsync(int listId, int userId, int listUserId);
    Task UnshareListWithUserAsync(int listId, int userId);
    Task UnshareListWithUserAsync(int listId, int userId, int listUserId);
}

public class DbListService : IDbListService
{
    public List AddList(string name, string username)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        return AddList(name, username, dbConn);
    }

    public List AddList(string name, string username, SQLiteConnection dbConn)
    {
        // Check if a user by this username exists.
        var user = DbUserService.GetUser(dbConn, "username = @username", new { username = username.ToLower() }) 
                   ?? throw ShoppingListerWebException.ApiUsernameNotFound(username);
        
        return AddList(name, user.Id, dbConn);
    }

    public List AddList(string name, int userId, SQLiteConnection dbConn)
    {
        var @params = new { name, userId };
        // Check if a list with this name and userId already exists.
        if (dbConn.Exists<List>("Name = @name AND UserId = @userId", @params))
            throw ShoppingListerWebException.ListExists(name, userId);

        // Create a new List record.
        var def = new CommandDefinition("INSERT INTO List (UserId, Name) VALUES (@userId, @name)", @params);
        if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateList();

        // Get List object.
        var listId = dbConn.LastInsertRowId;
        return dbConn.GetItem<List>("Id = @id", new { id = listId })
               ?? throw ShoppingListerWebException.CouldNotCreateList();
    }

    public Task<List> AddListAsync(NewListParameters userParams) => AddListAsync(userParams.Name, userParams.Username);

    public Task<List> AddListAsync(string name, string username)
    {
        var task = new Task<List>(() => AddList(name, username));
        task.Start();

        return task;
    }

    public Task<int> GetListCountAsync()
    {
        var task = new Task<int>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var count = dbConn.ExecuteScalar("SELECT COUNT(Id) FROM List");

            return Convert.ToInt32(count);
        });
        task.Start();

        return task;
    }

    public Task<List> GetListAsync(int listId)
    {
        var task = new Task<List>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return dbConn.GetItem<List>("Id = @listId", new { listId })
                   ?? throw ShoppingListerWebException.ApiListIdNotFound(listId);
        });
        task.Start();

        return task;
    }


    public Task<List[]> GetListsAsync(int offset, int count)
    {
        var task = new Task<List[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var parameters = new { offset, count };

            return dbConn.Query<List>("SELECT * FROM List LIMIT @count OFFSET @offset", parameters).ToArray();
        });
        task.Start();

        return task;
    }

    public Task<List[]> GetListsByUserIdAsync(int userId)
    {
        var task = new Task<List[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return dbConn.Query<List>("SELECT * FROM List WHERE UserId = @userId", new { userId }).ToArray();
        });
        task.Start();

        return task;
    }

    public Task<List[]> GetSharedListsByUserIdAsync(int userId)
    {
        var task = new Task<List[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return dbConn.Query<List>("SELECT * FROM List WHERE Id IN " +
                                      "(SELECT ListID FROM ListShare WHERE UserId = @userId)", new { userId }).ToArray();
        });
        task.Start();

        return task;
    }

    public Task<bool> UserHasAccessAsync(int id, int userId)
    {
        var task = new Task<bool>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var @params = new {id, userId};

            var def = new CommandDefinition($"SELECT COUNT(*) FROM List WHERE Id = @id AND UserId = @userId", @params);
            if (dbConn.ExecuteScalar<long>(def) == 1) return true;

            def = new CommandDefinition($"SELECT COUNT(*) FROM ListShare WHERE ListId = @id AND UserId = @userId", @params);
            return dbConn.ExecuteScalar<long>(def) == 1;
        });
        task.Start();

        return task;
    }

    public Task ShareListWithUserAsync(int listId, int userId)
        => Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            if (ShareExists(dbConn, listId, userId)) return;
            if (!DbUserService.ExistsById(dbConn, userId)) throw new NotImplementedException("User doesn't exist.");

            var def = new CommandDefinition("INSERT INTO ListShare (ListId, UserId) VALUES (@listId, @otherUserId)",
                                            new {listId, otherUserId = userId });
            if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateList();
        });

    public Task ShareListWithUserAsync(int listId, int userId, int listUserId)
        => Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            if (ShareExists(dbConn, listId, userId)) return;

            if (!dbConn.Exists<List>("Id = @listId AND UserId = @listUserId", new { listId, listUserId }))
                throw ShoppingListerWebException.NotAuthorised("You do not own the list.");

            if (!DbUserService.ExistsById(dbConn, userId)) throw new NotImplementedException("User doesn't exist.");

            var def = new CommandDefinition("INSERT INTO ListShare (ListId, UserId) VALUES (@listId, @otherUserId)",
                                            new {listId, otherUserId = userId });
            if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateList();
        });

    public Task UnshareListWithUserAsync(int listId, int userId)
        => Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            if (!ShareExists(dbConn, listId, userId)) return;

            dbConn.Execute("DELETE FROM ListShare WHERE ListId = @listId AND UserId = @userId", new { listId, userId });
        });

    public Task UnshareListWithUserAsync(int listId, int userId, int listUserId)
        => Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            if (!ShareExists(dbConn, listId, userId)) return;

            if (!dbConn.Exists<List>("Id = @listId AND UserId = @listUserId", new { listId, listUserId }))
                throw ShoppingListerWebException.NotAuthorised("You do not own the list.");

            dbConn.Execute("DELETE FROM ListShare WHERE ListId = @listId AND UserId = @userId", new { listId, userId });
        });

    public static void Create(SQLiteConnection dbConn, string name)
    {
        throw new NotImplementedException();
    }

    public static bool Exists(SQLiteConnection dbConn, int listId) => dbConn.Exists<List>("Id = @listId", new { listId });
    public static bool ShareExists(SQLiteConnection dbConn, int listId, int userId)
        => dbConn.Exists<ListShare>("ListId = @listId AND UserId = @userId", new { listId, userId });
}