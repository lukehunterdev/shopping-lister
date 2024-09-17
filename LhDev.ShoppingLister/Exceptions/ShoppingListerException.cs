namespace LhDev.ShoppingLister.Exceptions;

public class ShoppingListerException : Exception
{
    public int ExitCode { get; init; }


    #region Constructors

    protected ShoppingListerException(string? message) : base(message)
    {
    }

    protected ShoppingListerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion


    #region Settings errors

    private static ShoppingListerException NoSettings(string settingsName)
        => new($"Could not find '{settingsName}' settings section. Please check appsettings.json.") { ExitCode = 1 };

    public static ShoppingListerException NoJwtSettings()
        => NoSettings("Jwt");

    #endregion
}