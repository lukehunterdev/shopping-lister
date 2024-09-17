using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Pages;

public class LoginModel(IDbUserService dbUserService, IJwtService jwtService) : PageModel
{
    public string? ProblemText { get; private set; }

    public void OnGet()
    {
        var loginError = Request.Cookies[Program.CookieLoginError];
        if (!string.IsNullOrEmpty(loginError))
        {
            ProblemText = loginError;
            Response.Cookies.Delete(Program.CookieLoginError);
        }

        var jwt = Request.Cookies[Program.CookieSession];
        if (string.IsNullOrEmpty(jwt)) return;
        
        // Logged in, redirect
        Response.Redirect("/");
    }
}