using System.Text.Json;
using FeatureFlagApi.Models;
using FeatureFlagCore.Exceptions;

namespace FeatureFlagApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            FeatureFlagNotFoundException ex => (
                StatusCodes.Status404NotFound,
                new ErrorResponse { Type = "NotFound", Message = ex.Message }
            ),
            DuplicateFeatureFlagException ex => (
                StatusCodes.Status409Conflict,
                new ErrorResponse { Type = "Conflict", Message = ex.Message }
            ),
            DuplicateOverrideException ex => (
                StatusCodes.Status409Conflict,
                new ErrorResponse { Type = "Conflict", Message = ex.Message }
            ),
            OverrideNotFoundException ex => (
                StatusCodes.Status404NotFound,
                new ErrorResponse { Type = "NotFound", Message = ex.Message }
            ),
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse { Type = "ValidationError", Message = ex.Message, Errors = ex.Errors }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse { Type = "InternalError", Message = "An unexpected error occurred." }
            )
        };
        
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning("Request failed with {StatusCode}: {Message}", statusCode, exception.Message);
        }
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

// can be seperated in different file
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
