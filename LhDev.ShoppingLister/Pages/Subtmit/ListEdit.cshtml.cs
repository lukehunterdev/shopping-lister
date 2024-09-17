using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Pages.Subtmit;

[WebJwtAuthorise(RedirectUri = "/login")]
public class ListEditModel : PageModel
{
    //public List List { get; private set; } = null!;

    public async Task OnPostAsync(int id)
    {
        // var user = HttpContext.Items["User"] as User ?? throw ShoppingListerWebException.ShouldNotHappenMissingUserContext();
        // if (!await dbListService.UserHasAccessAsync(id, user.Id)) throw ShoppingListerWebException.NotAuthorisedUserViewList(user.Id, id);
        //
        // List = await dbListService.GetListAsync(id);
    }
}