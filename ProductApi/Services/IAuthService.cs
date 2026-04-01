using ProductApi.DTOs;

namespace ProductApi.Services;

public interface IAuthService
{
    /// <summary>Registers a new user. Returns null if email is already taken.</summary>
    Task<AuthResultDto?> RegisterAsync(RegisterDto dto);

    /// <summary>Authenticates and returns a JWT. Returns null on bad credentials.</summary>
    Task<AuthResultDto?> LoginAsync(LoginDto dto);

    /// <summary>Renew the JWT access token using a valid refresh token.</summary>
    Task<AuthResultDto?> RefreshAsync(TokenDto dto);
}

public class AuthResultDto
{
    public string Token        { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username     { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Role     { get; set; } = string.Empty;
}
