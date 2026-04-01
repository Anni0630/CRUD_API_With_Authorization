using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

/// <summary>
/// DTO provided by the client when requesting a new access token via refresh.
/// </summary>
public class TokenDto
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
