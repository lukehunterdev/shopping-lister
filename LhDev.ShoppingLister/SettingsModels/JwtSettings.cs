namespace LhDev.ShoppingLister.SettingsModels;

public interface IJwtSettings
{
    string Issuer { get; }
    string Audience { get; }
    string Key { get; }
    int Duration { get; }
}

public class JwtSettings //: IJwtSettings
{
    public string Issuer => "https://acme.com/";

    public string Audience => "https://acme.com/";

    public string Key { get; set; } = null!;

    public int Duration { get; set; } = 300;
}