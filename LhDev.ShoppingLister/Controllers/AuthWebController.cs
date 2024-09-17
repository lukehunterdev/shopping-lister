using LhDev.ShoppingLister.Attributes;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LhDev.ShoppingLister.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("web-api/auth")]
public class AuthWebController(IDbUserService dbUserService, IJwtService jwtService) : ControllerBase
{
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm]string userOrEmail, [FromForm]string password)
    {
        try
        {
            var user = userOrEmail.Contains('@')
                ? await dbUserService.AuthenticateUserByEmailAsync(userOrEmail, password)
                : await dbUserService.AuthenticateUserAsync(userOrEmail, password);

            var expires = DateTime.Now.AddDays(180);
            var token = jwtService.GenerateToken(user, expires);

            Response.Cookies.Append(Program.CookieSession, token, new CookieOptions {Expires = expires});

            return Redirect("/");
        }
        catch (ShoppingListerWebException ex)
        {
            Response.Cookies.Append(Program.CookieLoginError, ex.Message);

            return Redirect("/login");
        }
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        var cookie = Request.Cookies[Program.CookieSession];

        if (!string.IsNullOrEmpty(cookie)) Response.Cookies.Delete(Program.CookieSession);

        return Redirect("/login");
    }
}