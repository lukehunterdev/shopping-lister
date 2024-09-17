using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController(IDbUserService dbUserService, IJwtService jwtService) : ControllerBase
{
    /// <summary>
    /// Generates a JWT when given a valid username and password.
    /// </summary>
    /// <param name="auth">AuthParameters object containing username and password.</param>
    /// <returns>JWT token upon authentication.</returns>
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpPost]
    public async Task<IActionResult> Authenticate(AuthParameters auth)
    {
        var user = await dbUserService.AuthenticateUserAsync(auth.Username, auth.Password)
                   ?? throw ShoppingListerWebException.UserOrPasswordNotFound();

        if (await dbUserService.HasApiAccessAsync(user.Id)) throw ShoppingListerWebException.UserNoApi();

        var token = jwtService.GenerateToken(user);

        return Content(token);
    }

    /// <summary>
    /// Check the given JWT is valid.
    /// </summary>
    /// <returns>A string when JWT valid, otherwise an <see cref="UnauthorisedResponse"/>.</returns>
    [ApiJwtAuthorise]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(GeneralResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(GeneralResponse))]
    [HttpGet("validate")]
    public IActionResult ValidateToken() => Content("Token is valid.");
}