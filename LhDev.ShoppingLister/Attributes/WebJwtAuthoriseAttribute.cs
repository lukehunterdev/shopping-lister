using LhDev.ShoppingLister.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LhDev.ShoppingLister.Attributes;

/// <summary>
/// Use on a <see cref="PageModel"/> to require that page be protected by JWT cookie authorisation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WebJwtAuthoriseAttribute : JwtAuthoriseAttribute
{
    public string? RedirectUri { get; init; }

    protected override string Type => "cookie";

    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            base.OnAuthorization(context);
        }
        catch (ShoppingListerException ex)
        {
            if (ex.Message != "No token found." || string.IsNullOrEmpty(RedirectUri)) throw;

            context.Result = new RedirectResult(RedirectUri);
        }
    }
}