using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[WebJwtAuthorise]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("web-api/list")]
public class ListWebController(IDbListService dbListService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> New([FromForm] string listName)
    {
        var user = (User)HttpContext.Items["User"]!;
        await dbListService.AddListAsync(listName, user.Username);
        
        return Redirect("/");
    }

    [HttpPost("share")]
    public async Task<IActionResult> Share([FromForm] int listId, [FromForm] int userId)
    {
        var user = (User)HttpContext.Items["User"]!;
        await dbListService.ShareListWithUserAsync(listId, userId, user.Id);

        return Redirect($"~/list-edit/{listId}");
    }

    [HttpPost("unshare")]
    public async Task<IActionResult> Unshare([FromForm] int listId, [FromForm] int userId)
    {
        var user = (User)HttpContext.Items["User"]!;
        await dbListService.UnshareListWithUserAsync(listId, userId, user.Id);

        return Redirect($"~/list-edit/{listId}");
    }
}