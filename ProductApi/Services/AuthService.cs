using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Helpers;
using ProductApi.Models;

namespace ProductApi.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApplicationDbContext db, JwtHelper jwtHelper, ILogger<AuthService> logger)
    {
        _db        = db;
        _jwtHelper = jwtHelper;
        _logger    = logger;
    }

    public async Task<AuthResultDto?> RegisterAsync(RegisterDto dto)
    {
        // Check for duplicate email
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
        {
            _logger.LogWarning("Registration failed – email already exists: {Email}", dto.Email);
            return null;
        }

        // Normalise role (only "Admin" or "User" allowed)
        var role = dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        var user = new User
        {
            Username     = dto.Username,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = role
        };

        user.RefreshToken           = _jwtHelper.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("New user registered: {Email} ({Role})", user.Email, user.Role);

        return new AuthResultDto
        {
            Token        = _jwtHelper.GenerateToken(user),
            RefreshToken = user.RefreshToken,
            Username     = user.Username,
            Email        = user.Email,
            Role         = user.Role
        };
    }

    public async Task<AuthResultDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for email: {Email}", dto.Email);
            return null;
        }

        user.RefreshToken           = _jwtHelper.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User logged in: {Email}", user.Email);

        return new AuthResultDto
        {
            Token        = _jwtHelper.GenerateToken(user),
            RefreshToken = user.RefreshToken,
            Username     = user.Username,
            Email        = user.Email,
            Role         = user.Role
        };
    }

    public async Task<AuthResultDto?> RefreshAsync(TokenDto dto)
    {
        try
        {
            // Extract claims from the expired token
            var principal = _jwtHelper.GetPrincipalFromExpiredToken(dto.AccessToken);
            var email     = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

            if (email is null) return null;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Validate the refresh token
            if (user is null || 
                user.RefreshToken != dto.RefreshToken || 
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid refresh attempt for email: {Email}", email);
                return null;
            }

            // Rotate tokens
            user.RefreshToken           = _jwtHelper.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Tokens refreshed for user: {Email}", email);

            return new AuthResultDto
            {
                Token        = _jwtHelper.GenerateToken(user),
                RefreshToken = user.RefreshToken,
                Username     = user.Username,
                Email        = user.Email,
                Role         = user.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return null;
        }
    }
}
