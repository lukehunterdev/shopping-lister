using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Middleware;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[WebJwtAuthorise]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("ws")]
public class WebSocketController(IListWsService listWsService, IDbListService dbListService) : ControllerBase
{
    /// <summary>
    /// Endpoint to initialise a List WebSocket connection.
    /// </summary>
    /// <exception cref="ShoppingListerWebException"></exception>
    [HttpGet("list/{listId:int}")]
    public async Task ListInit(int listId)
    {
        var user = HttpContext.Items["User"] as User ?? throw ShoppingListerWebException.ShouldNotHappenMissingUserContext();
        if (!await dbListService.UserHasAccessAsync(listId, user.Id))
            throw ShoppingListerWebException.NotAuthorisedUserViewList(user.Id, listId);

        if (!HttpContext.WebSockets.IsWebSocketRequest) throw ShoppingListerWebException.ExpectingWebSocketRequest();

        await listWsService.ConnectedAsync(HttpContext.WebSockets, user, listId);
    }
}