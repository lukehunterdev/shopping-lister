using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LhDev.ShoppingLister.ApiModels;

/// <summary>
/// Provides a general message response.
/// </summary>
public class GeneralResponse
{
    /// <summary>
    /// HTTP status code of response.
    /// </summary>
    [Required]
    public int StatusCode { get; init; } = StatusCodes.Status200OK;

    /// <summary>
    /// Optional type of message e.g. Error.
    /// </summary>
    public string? Type { get; init; }

    [Required]
    public string Message { get; init; } = null!;

    public override string ToString() => JsonSerializer.Serialize(this);
}