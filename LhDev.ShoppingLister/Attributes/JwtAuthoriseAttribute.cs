using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LhDev.ShoppingLister.Attributes;

public abstract class JwtAuthoriseAttribute : Attribute, IAuthorizationFilter
{
    public bool RootOnly { get; init; }

    protected abstract string Type { get; }

    public virtual ShoppingListerException NotAuthorised(string message = null!)
        => ShoppingListerWebException.NotAuthorised(message);

    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        var source = (string?)context.HttpContext.Items["JwtSource"];

        if (string.IsNullOrEmpty(source)) throw NotAuthorised("No token found.");
        if (source != Type) throw NotAuthorised();
    
        // If a User object is attached to the context, we have a valid token.
        var user = (User?)context.HttpContext.Items["User"];
        if (user != null)
        {
            if (!RootOnly || user.Username == DbManager.RootUser) return;
    
            throw NotAuthorised("This method requires root access.");
        }
    
        // If there was an error authenticating the client, throw an exception and let the middleware handle the response cleanly.
        var failReason = context.HttpContext.Items["FailReason"];
        if (failReason is string s) throw NotAuthorised(s);
    
        // No token!
        throw NotAuthorised();
    }
}