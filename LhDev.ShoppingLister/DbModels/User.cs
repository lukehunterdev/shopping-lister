namespace LhDev.ShoppingLister.DbModels;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Api { get; set; }

    public bool Verified { get; set; }

    public bool Banned { get; set; }
}