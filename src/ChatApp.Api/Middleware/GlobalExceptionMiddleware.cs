using System.Text.Json;
using ChatApp.Application.Exceptions;

namespace ChatApp.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Response has already started; cannot write error response.");
                throw;
            }

            _logger.LogError(ex, "Unhandled exception caught by global middleware.");
            httpContext.Response.Clear();
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var statusCode = ex switch
            {
                UserAlreadyExistsException => StatusCodes.Status409Conflict,
                UserNotFoundByMailException => StatusCodes.Status401Unauthorized,
                IncorrectPasswordException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            var payload = statusCode == StatusCodes.Status500InternalServerError
                ? new { error = "An unexpected error occurred." }
                : new { error = ex.Message };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            await httpContext.Response.WriteAsync(json, httpContext.RequestAborted);
        }
    }
}