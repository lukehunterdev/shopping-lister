using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Pages;

[WebJwtAuthorise(RedirectUri = "/login")]
public class ListModel(IDbListService dbListService, IDbWorkingListService dbWorkingListService) : PageModel
{
    public List List { get; private set; } = null!;

    public long UnixTime { get; private set; } 


    public async Task OnGetAsync(int id)
    {
        UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var user = HttpContext.Items["User"] as User ?? throw ShoppingListerWebException.ShouldNotHappenMissingUserContext();
        if (!await dbListService.UserHasAccessAsync(id, user.Id)) throw ShoppingListerWebException.NotAuthorisedUserViewList(user.Id, id);

        List = await dbListService.GetListAsync(id);
    }
}