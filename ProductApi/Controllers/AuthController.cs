using Microsoft.AspNetCore.Mvc;
using ProductApi.DTOs;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user account.</summary>
    /// <remarks>
    /// Pass <c>role</c> as <b>"Admin"</b> or <b>"User"</b> (defaults to "User").
    ///
    /// Sample request:
    ///
    ///     POST /api/auth/register
    ///     {
    ///         "username": "john",
    ///         "email": "john@example.com",
    ///         "password": "Secret@123",
    ///         "role": "User"
    ///     }
    /// </remarks>
    /// <response code="201">Registration successful – returns JWT token.</response>
    /// <response code="400">Validation error or email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);

        if (result is null)
            return BadRequest(new { message = "Email is already registered." });

        return CreatedAtAction(nameof(Register), new { }, result);
    }

    /// <summary>Login with email and password to receive a JWT token.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email": "admin@example.com",
    ///         "password": "Admin@123"
    ///     }
    /// </remarks>
    /// <response code="200">Login successful – returns JWT token.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (result is null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }

    /// <summary>Refresh an expired JWT access token.</summary>
    /// <response code="200">Tokens refreshed.</response>
    /// <response code="400">Invalid refresh token or token swap attempt.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RefreshAsync(dto);

        if (result is null)
            return BadRequest(new { message = "Invalid access token or refresh token." });

        return Ok(result);
    }
}
