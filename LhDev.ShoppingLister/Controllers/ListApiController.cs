using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[ApiController]
[Route("api/list")]
public class ListApiController(IDbListService dbListService, IDbWorkingListService dbWorkingListService) : ControllerBase
{
    /// <summary>
    /// Gets a collection of lists.
    /// </summary>
    /// <returns>Upon success, a <see cref="PagedResponse{List}"/> object with the List objects.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<List>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet]
    public async Task<IActionResult> GetMany(int number = 0, int size = 100) =>
        Ok(new PagedResponse<List>
        {
            Page = number,
            PageSize = size,
            Total = (int)Math.Ceiling(await dbListService.GetListCountAsync() / (double)size),
            Records = await dbListService.GetListsAsync(number * size, size),
        });


    /// <summary>
    /// Gets list information by id number.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Upon success, a <see cref="List"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => Ok(await dbListService.GetListAsync(id));


    /// <summary>
    /// Creates a new list.
    /// </summary>
    /// <param name="parameters"><see cref="NewListParameters"/> object containing new list details.</param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost]
    public async Task<IActionResult> Post(NewListParameters parameters)
    {
        var list = await dbListService.AddListAsync(parameters);

        return this.OkGeneralResponse($"New list '{list.Name}' added.");
    }


    /// <summary>
    /// Add an item to the working list.
    /// </summary>
    /// <param name="listId"></param>
    /// <param name="itemId"></param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost("{listId:int}/item/{itemId:int}")]
    public async Task<IActionResult> AddItemToWorkingList(int listId, int itemId)
    {
        _ = await dbWorkingListService.AddItemToListAsync(listId, itemId);

        return this.OkGeneralResponse($"Item id {itemId} added to working list with id {listId}.");
    }


    /// <summary>
    /// Share a list with another user.
    /// </summary>
    /// <param name="listId"></param>
    /// <param name="userId"></param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost("{listId:int}/user/{userId:int}")]
    public async Task<IActionResult> Share(int listId, int userId)
    {
        var user = (User)HttpContext.Items["User"]!;
        await dbListService.ShareListWithUserAsync(listId, userId, user.Id);

        return this.OkGeneralResponse($"Shared list ID {listId} with user ID {userId}.");
    }
}