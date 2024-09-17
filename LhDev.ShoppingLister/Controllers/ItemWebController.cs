using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[WebJwtAuthorise]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("web-api/item")]
public class ItemWebController(IDbItemService dbItemService) : ControllerBase
{
    [HttpPost("edit")]
    public async Task<IActionResult> Edit([FromForm] int itemId, [FromForm] int listId, [FromForm] string itemName)
    {
        var user = (User)HttpContext.Items["User"]!;
        try
        {
            _ = await dbItemService.UpdateItemAsync(itemId, user.Id, itemName);
        }
        catch (ShoppingListerWebException ex)
        {
            Response.Cookies.Append(Program.ListEditError, ex.Message);
        }
        
        return Redirect($"~/list-edit/{listId}");
    }


    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromForm] int itemId, [FromForm] int listId)
    {
        var user = (User)HttpContext.Items["User"]!;
        try
        {
            await dbItemService.DeleteItemAsync(itemId, user.Id);
        }
        catch (ShoppingListerWebException ex)
        {
            Response.Cookies.Append(Program.ListEditError, ex.Message);
        }
        
        return Redirect($"~/list-edit/{listId}");
    }


}