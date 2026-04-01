using System.Net;
using System.Text.Json;

namespace ProductApi.Middleware;

/// <summary>
/// Global exception middleware – catches all unhandled exceptions and
/// returns a consistent JSON error envelope instead of the default HTML page.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentNullException  => (HttpStatusCode.BadRequest,            "A required argument was missing."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,     "Unauthorized access."),
            KeyNotFoundException   => (HttpStatusCode.NotFound,              "The requested resource was not found."),
            InvalidOperationException e => (HttpStatusCode.BadRequest,       e.Message),
            _                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = new
        {
            status  = (int)statusCode,
            error   = statusCode.ToString(),
            message
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}

/// <summary>Extension method for clean registration in Program.cs.</summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
