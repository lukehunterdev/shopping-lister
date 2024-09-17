namespace LhDev.ShoppingLister.DbModels;

public class Item
{
    public int Id { get; set; }

    public int ListId { get; set; }

    public string Name { get; set; } = null!;

    public int Usage { get; set; }
}