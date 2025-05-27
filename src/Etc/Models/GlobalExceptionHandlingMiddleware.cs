using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Etc.Models;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse();

        switch (exception)
        {
            case FileNotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "The requested file was not found.";
                break;
            case UnauthorizedAccessException:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized access.";
                break;
            case ArgumentException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;
            case InvalidOperationException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                break;
            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An error occurred while processing your request.";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = Activity.Current?.Id ?? string.Empty;
}