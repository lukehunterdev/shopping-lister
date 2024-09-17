namespace LhDev.ShoppingLister.ApiModels;

public class SetTicked
{
    public required int ItemId { get; init; }
    public required bool Ticked { get; init; }
}