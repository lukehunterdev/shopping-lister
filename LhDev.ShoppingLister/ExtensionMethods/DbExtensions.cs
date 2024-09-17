using Dapper;
using System.Data.SQLite;
using System.Xml.Linq;

namespace LhDev.ShoppingLister.ExtensionMethods;

public static class DbExtensions
{
    #region Get DB items

    private static string SqlGetItems<TDbObj>(string? whereClause)
    {
        var sql = $"SELECT * FROM {typeof(TDbObj).Name} ";
        if (!string.IsNullOrEmpty(whereClause)) sql = $"{sql} WHERE {whereClause}";

        return sql;
    }


    public static TDbObj? GetItem<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => dbConn.Query<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>(whereClause), parameters)).FirstOrDefault();

    public static async Task<TDbObj?> GetItemAsync<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => (await dbConn.QueryAsync<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>(whereClause), parameters))).FirstOrDefault();


    public static TDbObj? GetItemById<TDbObj>(this SQLiteConnection dbConn, int id)
        => dbConn.Query<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>("ID = @id"), new { id })).FirstOrDefault();

    public static async Task<TDbObj?> GetItemByIdAsync<TDbObj>(this SQLiteConnection dbConn, int id)
        => (await dbConn.QueryAsync<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>("ID = @id"), new { id }))).FirstOrDefault();


    public static TDbObj? GetItemByName<TDbObj>(this SQLiteConnection dbConn, string name)
        => dbConn.Query<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>("Name = @name"), new { name }))
            .FirstOrDefault();

    public static async Task<TDbObj?> GetItemByNameAsync<TDbObj>(this SQLiteConnection dbConn, string name)
        => (await dbConn.QueryAsync<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>("Name = @name"), new { name })))
            .FirstOrDefault();


    public static TDbObj[] GetItems<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => dbConn.Query<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>(whereClause), parameters)).ToArray();

    public static async Task<TDbObj[]> GetItemsAsync<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => (await dbConn.QueryAsync<TDbObj>(new CommandDefinition(SqlGetItems<TDbObj>(whereClause), parameters))).ToArray();

    #endregion



    #region Count / exists

    private static string SqlItemCount<TDbObj>(string? whereClause)
    {
        var sql = $"SELECT COUNT(*) FROM {typeof(TDbObj).Name} ";
        if (!string.IsNullOrEmpty(whereClause)) sql = $"{sql} WHERE {whereClause}";

        return sql;
    }


    public static long Count<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => dbConn.ExecuteScalar<long>(new CommandDefinition(SqlItemCount<TDbObj>(whereClause), parameters));

    public static Task<long> CountAsync<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => dbConn.ExecuteScalarAsync<long>(new CommandDefinition(SqlItemCount<TDbObj>(whereClause), parameters));


    public static bool Exists<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => dbConn.ExecuteScalar<long>(new CommandDefinition(SqlItemCount<TDbObj>(whereClause), parameters)) == 1;

    public static async Task<bool> ExistsAsync<TDbObj>(this SQLiteConnection dbConn, string? whereClause, object? parameters = null)
        => await dbConn.ExecuteScalarAsync<long>(new CommandDefinition(SqlItemCount<TDbObj>(whereClause), parameters)) == 1;


    public static bool ExistsById<TDbObj>(this SQLiteConnection dbConn, int id)
        => dbConn.ExecuteScalar<long>(new CommandDefinition(SqlItemCount<TDbObj>("ID = @id"), new { id })) == 1;

    public static async Task<bool> ExistsByIdAsync<TDbObj>(this SQLiteConnection dbConn, int id)
        => await dbConn.ExecuteScalarAsync<long>(new CommandDefinition(SqlItemCount<TDbObj>("ID = @id"), new { id })) == 1;


    public static bool ExistsByName<TDbObj>(this SQLiteConnection dbConn, string name)
        => dbConn.ExecuteScalar<long>(new CommandDefinition(SqlItemCount<TDbObj>("Name = @name"), new { name })) == 1;

    public static async Task<bool> ExistsByNameAsync<TDbObj>(this SQLiteConnection dbConn, string name)
        => await dbConn.ExecuteScalarAsync<long>(new CommandDefinition(SqlItemCount<TDbObj>("Name = @name"), new { name })) == 1;

    #endregion
}