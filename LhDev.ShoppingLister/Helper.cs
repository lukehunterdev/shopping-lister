using LhDev.ShoppingLister.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister;

public static class Helper
{
    public static IActionResult OkGeneralResponse(this ControllerBase cb, string message) 
        => cb.Ok(new GeneralResponse { Message = message });
}