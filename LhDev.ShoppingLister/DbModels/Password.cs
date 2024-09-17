namespace LhDev.ShoppingLister.DbModels;

public class Password
{
    public int UserId { get; set; }

    public string Salt { get; set; } = null!;

    public string Hash { get; set; } = null!;
}