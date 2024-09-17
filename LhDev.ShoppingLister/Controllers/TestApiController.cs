using System;
using LhDev.ShoppingLister.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LhDev.ShoppingLister.Controllers;

[ApiController]
[Route("api/test")]
public class TestApiController
{
    /// <summary>
    /// Throws a general exception to test exception handling middleware.
    /// </summary>
    /// <returns>Never returns.</returns>
    /// <exception cref="Exception">This is a general exception.</exception>
    [HttpGet("generalException")]
    public IActionResult ThrowGeneralException()
    {
        throw new Exception("This is a general exception.");
    }


    /// <summary>
    /// Throws a not implemented exception to test exception handling middleware.
    /// </summary>
    /// <returns>Never returns.</returns>
    /// <exception cref="NotImplementedException">This is a not implemented exception.</exception>
    [HttpGet("notImplementedException")]
    public IActionResult ThrowNotImplementedException()
    {
        throw new NotImplementedException();
    }



    /// <summary>
    /// Throws a Shopping Lister exception to test exception handling middleware.
    /// </summary>
    /// <returns>Never returns.</returns>
    /// <exception cref="ShoppingListerWebException">This is a Shopping Lister exception.</exception>
    [HttpGet("shoppingListerException")]
    public IActionResult ThrowShoppingListerException()
    {
        throw ShoppingListerWebException.TestException();
    }
}