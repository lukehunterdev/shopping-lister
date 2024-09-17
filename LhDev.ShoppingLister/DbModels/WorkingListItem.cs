namespace LhDev.ShoppingLister.DbModels;

public class WorkingListItem
{
    public int ListId { get; set; }

    public int ItemId { get; set; }

    public int Ordinal { get; set; }

    public bool Ticked { get; set; }
}