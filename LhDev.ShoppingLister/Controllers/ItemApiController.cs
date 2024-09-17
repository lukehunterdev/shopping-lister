using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[ApiController]
[Route("api/item")]
public class ItemApiController(IDbItemService dbItemService) : ControllerBase
{
    /// <summary>
    /// Gets a collection of items.
    /// </summary>
    /// <returns>Upon success, a <see cref="PagedResponse{Item}"/> object with the Item objects.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<Item>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet]
    public async Task<IActionResult> GetMany(int number = 0, int size = 100) =>
        Ok(new PagedResponse<Item>
        {
            Page = number,
            PageSize = size,
            Total = (int)Math.Ceiling(await dbItemService.GetItemCountAsync() / (double)size),
            Records = await dbItemService.GetItemsAsync(number * size, size),
        });


    /// <summary>
    /// Gets list information by id number.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Upon success, a <see cref="Item"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Item))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await dbItemService.GetItemAsync(id));


    /// <summary>
    /// Creates a new item.
    /// </summary>
    /// <param name="parameters"><see cref="NewItemParameters"/> object containing new item details.</param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost]
    public async Task<IActionResult> Post(NewItemParameters parameters)
    {
        var item = await dbItemService.AddItemAsync(parameters);

        return this.OkGeneralResponse($"New item '{item.Name}' added.");
    }
}