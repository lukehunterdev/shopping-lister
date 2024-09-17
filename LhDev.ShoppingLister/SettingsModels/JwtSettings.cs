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
    public string Issuer => "https://idoxgroup.com/";

    public string Audience => "https://idoxgroup.com/";

    public string Key { get; set; } = null!;

    public int Duration { get; set; } = 300;
}