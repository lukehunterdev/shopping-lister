using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Pages;

[WebJwtAuthorise(RedirectUri = "/login")]
public class ListEditModel(IDbListService dbListService, IDbItemService dbItemService, IDbUserService dbUserService) : PageModel
{
    public string? ProblemText { get; private set; }

    public List List { get; private set; } = null!;
    public Item[] Items { get; private set; } = null!;
    public User[] SharedUsers { get; private set; } = null!;
    public User[] OtherUsers { get; private set; } = null!;

    public long UnixTime { get; private set; }

    public async Task OnGetAsync(int id)
    {
        var someError = Request.Cookies[Program.ListEditError];
        if (!string.IsNullOrEmpty(someError))
        {
            ProblemText = someError;
            Response.Cookies.Delete(Program.ListEditError);
        }
        
        UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var user = HttpContext.Items["User"] as User ?? throw ShoppingListerWebException.ShouldNotHappenMissingUserContext();
        if (!await dbListService.UserHasAccessAsync(id, user.Id)) throw ShoppingListerWebException.NotAuthorisedUserViewList(user.Id, id);
        
        List = await dbListService.GetListAsync(id);
        Items = await dbItemService.GetItemsByListIdAsync(id);
        SharedUsers = await dbUserService.GetUsersBySharedListAsync(id);
        OtherUsers = await dbUserService.GetUsersNotSharedListAsync(id, user.Id);
    }
}