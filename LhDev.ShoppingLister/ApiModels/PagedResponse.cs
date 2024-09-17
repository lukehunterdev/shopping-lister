namespace LhDev.ShoppingLister.ApiModels;

public class PagedResponse<T>
{
    public int Page { get; set; }

    public int Total { get; set; }

    public int PageSize { get; set; }

    public T[] Records { get; set; } = null!;
}