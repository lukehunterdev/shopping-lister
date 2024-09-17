using System.ComponentModel.DataAnnotations;

namespace LhDev.ShoppingLister.ApiModels;

/// <summary>
/// Used to add a new user to the system.
/// </summary>
public class NewItemParameters
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string ListName { get; set; } = null!;
}