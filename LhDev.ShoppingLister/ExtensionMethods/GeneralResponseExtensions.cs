using LhDev.ShoppingLister.ApiModels;
using LhDev.ShoppingLister.Exceptions;

namespace LhDev.ShoppingLister.ExtensionMethods;

public static class GeneralResponseExtensions
{
    public static GeneralResponse NotImplementedAsGeneralResponse() => new()
    {
        StatusCode = StatusCodes.Status501NotImplemented,
        Message = "This feature is not implemented.",
        Type = "Error",
    };

    public static GeneralResponse AsGeneralResponse(this Exception ex) => new()
    {
        StatusCode = StatusCodes.Status500InternalServerError,
        Message = ex.Message,
        Type = "Error",
    };

    public static GeneralResponse AsGeneralResponse(this ShoppingListerWebException ex) => new()
    {
        StatusCode = ex.StatusCode,
        Message = ex.Message,
        Type = "Error",
    };
}