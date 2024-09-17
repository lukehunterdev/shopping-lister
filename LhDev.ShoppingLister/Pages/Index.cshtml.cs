using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Pages;

[WebJwtAuthorise(RedirectUri = "/login")]
public class IndexModel(ILogger<IndexModel> logger, IDbListService dbListService) : PageModel
{
    public List[] Lists { get; private set; } = null!;

    public List[] SharedLists { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        var user = (User)HttpContext.Items["User"]!;
        Lists = await dbListService.GetListsByUserIdAsync(user.Id);
        SharedLists = await dbListService.GetSharedListsByUserIdAsync(user.Id);
    }
}
