using System.ComponentModel.DataAnnotations;

namespace LhDev.ShoppingLister.ApiModels;

public class WorkingItem
{
    [Required]
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = null!;

    [Required] 
    public bool Ticked { get; set; }
}