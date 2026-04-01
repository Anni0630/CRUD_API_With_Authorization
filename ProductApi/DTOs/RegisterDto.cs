using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Accepted values: "Admin" or "User" (defaults to "User")</summary>
    public string Role { get; set; } = "User";
}
