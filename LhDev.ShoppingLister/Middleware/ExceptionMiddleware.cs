using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.ExtensionMethods;

namespace LhDev.ShoppingLister.Middleware;

/// <summary>
/// Middleware for clean exception handling in this application.
/// </summary>
/// <param name="next">A function that can process an HTTP request.</param>
/// <remarks>
/// Using this instead of <see cref="ExceptionHandlerExtensions.UseExceptionHandler(IApplicationBuilder)"/> is about 10x more
/// performant in my own testing.
/// </remarks>
public class ExceptionMiddleware(RequestDelegate next)
{
    private const string JsonContentType = "application/json";

    /// <summary>
    /// Is invoked for every web request. This middleware simply invokes the next stage but traps exceptions.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ShoppingListerWebException ex)
        {
            await HandleApiExceptionAsync(httpContext, ex);
        }
        catch (NotImplementedException)
        {
            await HandleNotImplementedExceptionAsync(httpContext);
        }
        catch (Exception ex)
        {
            await HandleUncaughtExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleApiExceptionAsync(HttpContext context, ShoppingListerWebException ex)
    {
        context.Response.ContentType = JsonContentType;
        context.Response.StatusCode = ex.StatusCode;

        await context.Response.WriteAsync(ex.AsGeneralResponse().ToString());
    }

    private static async Task HandleNotImplementedExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = JsonContentType;
        context.Response.StatusCode = StatusCodes.Status501NotImplemented;

        await context.Response.WriteAsync(GeneralResponseExtensions.NotImplementedAsGeneralResponse().ToString());
    }

    private static async Task HandleUncaughtExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = JsonContentType;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsync(ex.AsGeneralResponse().ToString());
    }
}