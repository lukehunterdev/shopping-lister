using System.Collections.ObjectModel;
using Dapper;
using System.Data.SQLite;
using System.Reflection;
using LhDev.ShoppingLister.Services;

namespace LhDev.ShoppingLister;

public static class DbManager
{
    public static ReadOnlyDictionary<string, string> DbValues { get; private set; }

    public static string RootUser => DbValues["RootUser"];

    public static int ThisDbVersion { get;  } = 2;

    public static string DbPath { get; private set; } = null!;

    public static string ConnStr => $"Data Source={DbPath};Version=3;";

    public static SQLiteConnection CreateDbConnection() => new(ConnStr);

    /// <summary>
    /// Call on application startup to check that the SQLite database already exists. If not create it here.
    /// </summary>
    public static void InitDatabase()
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
                       ?? throw new Exception("Could not find executing path.");
        if (basePath.EndsWith(Path.DirectorySeparatorChar)) basePath = basePath[..^1];
        var baseDir = $"{basePath}{Path.DirectorySeparatorChar}data";
        basePath = $"{baseDir}{Path.DirectorySeparatorChar}sl.sqlite";

        DbPath = basePath;
        var createDb = !File.Exists(DbPath);
        if (createDb && !Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

        if (createDb) SQLiteConnection.CreateFile(DbPath);
        
        using var dbConn = CreateDbConnection();
        dbConn.Open();

        if (createDb)
        {
            CreateDb(dbConn);
            CreateRootUser(dbConn);
        }

        ReadDbValues(dbConn);
        CheckDbVersionAndUpgrade(dbConn);
    }

    private static void CheckDbVersionAndUpgrade(SQLiteConnection dbConn)
    {
        while (true)
        {
            var dbVer = int.Parse(DbValues["DbVersion"]);

            if (dbVer == 1) UpgradeFrom1To2(dbConn);
            else if (dbVer == ThisDbVersion) return;

            ReadDbValues(dbConn);
        }
    }

    private static void SetNewDbVersion(SQLiteConnection dbConn, int version)
    {
        dbConn.Execute("UPDATE SlInfo SET Value = @ver", new { ver = version.ToString() });
    }

    private static void UpgradeFrom1To2(SQLiteConnection dbConn)
    {
        dbConn.Execute(GetResourceString("UpgradeFrom1To2.sql"));
        SetNewDbVersion(dbConn, 2);
    }

    private static void CreateRootUser(SQLiteConnection dbConn)
    {
        // Generate username and password
        var username = "root_" + Guid.NewGuid().ToString().Replace("-", "")[..6];
        var password = Guid.NewGuid().ToString().Replace("-", "");
            
        // Add to database
        var userService = new DbUserService();
        var rootUser = userService.AddUser(username, "Admin", $"{username}@admin", password, dbConn);
        userService.SetUserAsVerified(rootUser.Id, dbConn);
        userService.SetUserApiAccess(rootUser.Id, true, dbConn);
        dbConn.Execute("INSERT INTO SlInfo VALUES ('RootUser', @username)", new {username});

        // Inform user
        Console.WriteLine("*** ROOT PASSWORD CHANGE IMMEDIATELY! ***\n");
        Console.WriteLine("This message will only appear once.\n");
        Console.WriteLine($"Username is '{username}'.\nPassword is '{password}'.\n");
        Console.WriteLine("*** ROOT PASSWORD CHANGE IMMEDIATELY! ***\n");
    }

    private static void CreateDb(SQLiteConnection dbConn)
    {
        dbConn.Execute(GetResourceString("CreateDb.sql"));
    }

    private static void ReadDbValues(SQLiteConnection dbConn)
    {
        DbValues = dbConn.Query<(string, string)>("SELECT Name, Value FROM SlInfo")
            .ToDictionary(r => r.Item1, r => r.Item2)
            .AsReadOnly();
    }

    private static string GetResourceString(string name)
    {
        var ass = Assembly.GetExecutingAssembly();
        var resources = ass.GetManifestResourceNames().Where(r => r.EndsWith(name)).ToArray();

        if (resources.Length > 1)
            throw new Exception($"Resource name '{resources}' is ambiguous.");
        if (resources.Length < 1)
            throw new Exception($"Resource name '{resources}' could not be found.");
        var res = resources[0];

        using var stream = ass.GetManifestResourceStream(res);
        if (stream == null) throw new Exception($"Could not open stream for found resource '{res}'.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}