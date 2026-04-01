namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Role is either "Admin" or "User"</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string Role { get; set; } = "User";

    // ── Refresh Token Support ──────────────────────────────────────────
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
