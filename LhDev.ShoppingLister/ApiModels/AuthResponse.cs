using LhDev.ShoppingLister.DbModels;

namespace LhDev.ShoppingLister.ApiModels;

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }


    public AuthResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        Token = token;
    }
}