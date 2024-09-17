using System.Data.SQLite;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Dapper;
using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.ExtensionMethods;

namespace LhDev.ShoppingLister.Services;

public interface IDbUserService
{
    Task<User> AuthenticateUserAsync(string username, string password);
    Task<User> AuthenticateUserByEmailAsync(string email, string password);
    Task<User> AddUserAsync(NewUserParameters userParams);
    Task<User> AddUserAsync(string username, string name, string email, string password);
    Task<bool> HasApiAccessAsync(int userId);
    Task<int> GetUserCountAsync();
    Task<User[]> GetUsersAsync(int offset, int count);
    Task<User[]> GetUsersBySharedListAsync(int listId);
    Task<User[]> GetUsersNotSharedListAsync(int listId, int thisUserId);
    Task<User> GetUserAsync(int userId);
    Task<User> GetUserAsync(string username);
    Task SetUserAsVerifiedAsync(int userId);

}

public class DbUserService : IDbUserService
{
    public Task<User> AuthenticateUserAsync(string username, string password)
    {
        var task = new Task<User>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            // Get User object or error out.
            var user = GetUser(dbConn, "Username = @username", new { username = username.ToLower() })
                       ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Get Password object or error out.
            var pass = GetPassword(dbConn, "UserId = @userId", new { userId = user.Id })
                       ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Calculate password hash, and check if the hashes match.
            if (CalculateHash(password, pass.Salt) != pass.Hash) throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Ensure the user is verified and not banned.
            if (!user.Verified) throw ShoppingListerWebException.UserNotVerified();
            if (user.Banned) throw ShoppingListerWebException.UserBanned();

            // Validated, return user.
            return user;
        });

        task.Start();

        return task;
    }

    public Task<User> AuthenticateUserByEmailAsync(string email, string password)
    {
        var task = new Task<User>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            // Get User object or error out.
            var user = GetUser(dbConn, "Email = @email", new { email = email.ToLower() })
                       ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Get Password object or error out.
            var pass = GetPassword(dbConn, "UserId = @userId", new { userId = user.Id })
                       ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Calculate password hash, and check if the hashes match.
            if (CalculateHash(password, pass.Salt) != pass.Hash) throw ShoppingListerWebException.UserOrPasswordNotFound();

            // Ensure the user is verified and not banned.
            if (!user.Verified) throw ShoppingListerWebException.UserNotVerified();
            if (user.Banned) throw ShoppingListerWebException.UserBanned();

            // Validated, return user.
            return user;
        });

        task.Start();

        return task;
    }

    public User AddUser(NewUserParameters userParams)
        => AddUser(userParams.Username, userParams.Name, userParams.Email, userParams.Password);

    public User AddUser(NewUserParameters userParams, SQLiteConnection dbConn)
        => AddUser(userParams.Username, userParams.Name, userParams.Email, userParams.Password, dbConn);

    public User AddUser(string username, string name, string email, string password)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        return AddUser(username, name, email, password, dbConn);
    }

    public User AddUser(string username, string name, string email, string password, SQLiteConnection dbConn)
    {
        // Check if a user by this username or email exists.
        if (UserExists(dbConn, "username = @username", new { username = username.ToLower() }))
            throw ShoppingListerWebException.UserExists(username);
        if (UserExists(dbConn, "email = @email", new { email = email.ToLower() }))
            throw ShoppingListerWebException.EmailExists(email);

        // Create a new User record.
        var def = new CommandDefinition("INSERT INTO User (Username, Name, Email) VALUES (@username, @name, @email)",
            new { username = username.ToLower(), name, email = email.ToLower() });
        if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreateUser();

        // Get User object.
        var userId = dbConn.LastInsertRowId;
        var user = GetUser(dbConn, "Id = @id", new { id = userId })
                   ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

        // Create a new Password record.
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128));
        var hash = CalculateHash(password, salt);
        def = new CommandDefinition("INSERT INTO Password (UserId, Salt, Hash) VALUES (@userId, @salt, @hash)",
            new { userId, salt, hash });
        if (dbConn.Execute(def) != 1) throw ShoppingListerWebException.CouldNotCreatePassword();

        return user;
    }


    public Task<User> AddUserAsync(NewUserParameters userParams)
        => AddUserAsync(userParams.Username, userParams.Name, userParams.Email, userParams.Password);

    public Task<User> AddUserAsync(string username, string name, string email, string password)
    {
        var task = new Task<User>(() => AddUser(username, name, email, password));
        task.Start();

        return task;
    }

    public Task<bool> HasApiAccessAsync(int userId)
    {
        var task = new Task<bool>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var def = new CommandDefinition("SELECT COUNT(Id) FROM User WHERE Id = @userId AND Api = @api", 
                new { userId, api = true });

            return dbConn.Execute(def) == 1;
        });
        task.Start();

        return task;
    }

    public Task<int> GetUserCountAsync()
    {
        var task = new Task<int>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var count = dbConn.ExecuteScalar("SELECT COUNT(Id) FROM User");

            return Convert.ToInt32(count);
        });
        task.Start();

        return task;
    }

    public Task<User[]> GetUsersAsync(int offset, int count)
    {
        var task = new Task<User[]>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var parameters = new { offset, count };

            return dbConn.Query<User>("SELECT * FROM User LIMIT @count OFFSET @offset", parameters).ToArray();
        });
        task.Start();

        return task;
    }

    public Task<User[]> GetUsersBySharedListAsync(int listId) =>
        Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var parameters = new { listId };

            return dbConn.Query<User>("SELECT * FROM User WHERE Id IN " +
                                      "(SELECT UserId FROM ListShare WHERE ListId = @listId)", parameters).ToArray();
        });

    public Task<User[]> GetUsersNotSharedListAsync(int listId, int thisUserId) =>
        Task.Run(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            var parameters = new { listId, thisUserId };

            return dbConn.Query<User>("SELECT * FROM User WHERE Api = FALSE AND Id <> @thisUserId AND Id NOT IN " +
                                      "(SELECT UserId FROM ListShare WHERE ListId = @listId)", parameters).ToArray();
        });

    public Task<User> GetUserAsync(int userId)
    {
        var task = new Task<User>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return GetUser(dbConn, "Id = @userId", new {userId}) 
                   ?? throw ShoppingListerWebException.ApiUserIdNotFound(userId);
        });
        task.Start();

        return task;
    }
    
    public Task<User> GetUserAsync(string username)
    {
        var task = new Task<User>(() =>
        {
            using var dbConn = DbManager.CreateDbConnection();
            dbConn.Open();

            return GetUser(dbConn, "Username = @username", new {username}) 
                   ?? throw ShoppingListerWebException.ApiUsernameNotFound(username);
        });
        task.Start();

        return task;
    }

    public Task SetUserAsVerifiedAsync(int userId)
    {
        var task = new Task(() => SetUserAsVerified(userId));
        task.Start();

        return task;
    }


    public void SetUserAsVerified(int userId, SQLiteConnection dbConn)
    {
        var parameters = new {userId};

        if (!UserExists(dbConn, "Id = @userId", parameters)) 
            throw ShoppingListerWebException.CouldNotFindUserId(userId);

        var def = new CommandDefinition("UPDATE User SET Verified = TRUE WHERE Id = @userId", parameters);
        // if (dbConn.Execute(def) != 1)
        //     throw ShoppingListerWebException.CouldNotUpdateUser(userId, "Trying to verify user.");
        dbConn.Execute(def);
    }

    public void SetUserAsVerified(int userId)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        SetUserAsVerified(userId, dbConn);
    }


    public void SetUserApiAccess(int userId, bool apiAccess, SQLiteConnection dbConn)
    {
        var parameters = new {userId};

        if (!UserExists(dbConn, "Id = @userId", parameters)) 
            throw ShoppingListerWebException.CouldNotFindUserId(userId);

        var def = new CommandDefinition("SELECT COUNT(Id) FROM User WHERE Id = @userId AND Api = TRUE", parameters);
        var hasApiAlready = dbConn.ExecuteScalar<long>(def) == 1;

        // If no changes need to be made to the database, don't bother doing it.
        if (apiAccess ^ !hasApiAlready) return;

        def = new CommandDefinition($"UPDATE User SET Api = {apiAccess.ToString().ToUpper()} WHERE Id = @userId", parameters);
        if (dbConn.Execute(def) != 1)
            throw ShoppingListerWebException.CouldNotUpdateUser(userId, "Trying to grant or revoke API access.");
    }

    public void SetUserApiAccess(int userId, bool apiAccess)
    {
        using var dbConn = DbManager.CreateDbConnection();
        dbConn.Open();

        SetUserApiAccess(userId, apiAccess, dbConn);
    }




    public static bool ExistsById(SQLiteConnection dbConn, int userId) => dbConn.Exists<User>("Id = @userId", new {userId});
    


    internal static bool UserExists(SQLiteConnection dbConn, string whereClause, object? parameters = null)
    {
        var def = new CommandDefinition($"SELECT COUNT(Id) FROM User WHERE {whereClause}", parameters);

        return dbConn.ExecuteScalar<long>(def) == 1;
    }

    internal static User? GetUser(SQLiteConnection dbConn, string whereClause, object? parameters = null)
    {
        var def = new CommandDefinition($"SELECT * FROM User WHERE {whereClause}", parameters);

        return dbConn.Query<User>(def).FirstOrDefault() ;
    }

    private static Password? GetPassword(SQLiteConnection dbConn, string whereClause, object? parameters = null)
    {
        var def = new CommandDefinition($"SELECT * FROM Password WHERE {whereClause}", parameters);

        return dbConn.Query<Password>(def).FirstOrDefault();
    }

    private static string CalculateHash(string password, string salt)
    {
        using var sha = SHA512.Create() ?? throw new Exception();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{password}#{salt}"));
        var sb = new StringBuilder();

        foreach (var b in bytes) sb.Append($"{b:X2}");
        return sb.ToString();
    }
}