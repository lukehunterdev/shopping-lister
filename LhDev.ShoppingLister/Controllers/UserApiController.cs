using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[ApiController]
[Route("api/user")]
public class UserApiController(IDbUserService dbUserService, IJwtService jwtService) : ControllerBase
{
    /// <summary>
    /// Gets a collection of users.
    /// </summary>
    /// <param name="number"></param>
    /// <param name="size"></param>
    /// <returns>Upon success, a <see cref="PagedResponse{User}"/> object with the User objects</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<User>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet]
    public async Task<IActionResult> GetMany(int number = 0, int size = 100) =>
        Ok(new PagedResponse<User>
        {
            Page = number,
            PageSize = size,
            Total = (int)Math.Ceiling(await dbUserService.GetUserCountAsync() / (double)size),
            Records = await dbUserService.GetUsersAsync(number * size, size),
        });


    /// <summary>
    /// Gets user information by id number.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Upon success, a <see cref="User"/> object.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await dbUserService.GetUserAsync(id));


    /// <summary>
    /// Gets user information by username.
    /// </summary>
    /// <param name="username"></param>
    /// <returns>Upon success, a <see cref="User"/> object.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet("username/{username}")]
    public async Task<IActionResult> Get(string username) => Ok(await dbUserService.GetUserAsync(username));


    /// <summary>
    /// Creates a new user. The user will not be verified.
    /// </summary>
    /// <param name="userParameters"><see cref="NewUserParameters"/> object containing new user details.</param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost]
    public async Task<IActionResult> Post(NewUserParameters userParameters)
    {
        var user = await dbUserService.AddUserAsync(userParameters);

        return this.OkGeneralResponse($"New user '{userParameters.Username}' added.");
    }


    /// <summary>
    /// Sets a user as verified. You need to be the root user to do this.
    /// </summary>
    /// <param name="id">NewUserParameters object containing new user details.</param>
    /// <returns>Upon success, a <see cref="GeneralResponse"/> with a message.</returns>
    [ApiJwtAuthorise(RootOnly = true)]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPut("verify/{id}")]
    public async Task<IActionResult> SetVerified(int id)
    {
        await dbUserService.SetUserAsVerifiedAsync(id);

        return this.OkGeneralResponse($"User ID {id} is now verified.");
    }
}