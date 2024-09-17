using System.ComponentModel.DataAnnotations;

namespace LhDev.ShoppingLister.ApiModels;

/// <summary>
/// Used to provide username and password to authenticate client and provide a JWT.
/// </summary>
public class AuthParameters
{
    /// <summary>
    /// Username
    /// </summary>
    [Required]
    public string Username { get; set; } = null!;


    /// <summary>
    /// Password
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}