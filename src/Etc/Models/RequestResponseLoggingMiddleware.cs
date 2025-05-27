using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Etc.Models;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    // Static file extensions and paths to exclude from logging
    private static readonly HashSet<string> StaticFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/swagger", "/health", "/favicon.ico"
    };
    
    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files and certain paths
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequest(context);
        
        // Capture original response body stream
        var originalResponseBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            await LogResponse(context, stopwatch.ElapsedMilliseconds);
            
            // Copy the response back to the original stream
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
    }
    
    private static bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;

        // Skip if path starts with excluded paths
        if (ExcludedPaths.Any(excluded => pathValue.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Skip if path has static file extension
        var extension = Path.GetExtension(pathValue);
        if (!string.IsNullOrEmpty(extension) && StaticFileExtensions.Contains(extension))
            return true;

        return false;
    }

    private async Task LogRequest(HttpContext context)
    {
        try
        {
            var request = context.Request;
            var requestBody = string.Empty;

            // Read request body for POST/PUT requests (be careful with large files)
            if (request.Method == "POST" || request.Method == "PUT")
            {
                if (request.ContentLength.HasValue && request.ContentLength < 1024 * 10) // Only log small payloads
                {
                    request.EnableBuffering();
                    var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                    await request.Body.ReadAsync(buffer, 0, buffer.Length);
                    requestBody = Encoding.UTF8.GetString(buffer);
                    request.Body.Position = 0;
                }
            }

            _logger.LogInformation("HTTP Request: {Method} {Path} {QueryString} - Body: {RequestBody}",
                request.Method,
                request.Path,
                request.QueryString,
                string.IsNullOrEmpty(requestBody) ? "[Empty or Large]" : requestBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log request details");
        }
    }

    private async Task LogResponse(HttpContext context, long elapsedMs)
    {
        try
        {
            var response = context.Response;
            var responseBody = string.Empty;

            // Read response body (be careful with large files)
            if (response.Body.Length < 1024 * 10) // Only log small responses
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                responseBody = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
            }

            _logger.LogInformation("HTTP Response: {StatusCode} - {ElapsedMs}ms - Body: {ResponseBody}",
                response.StatusCode,
                elapsedMs,
                string.IsNullOrEmpty(responseBody) ? "[Empty or Large]" : responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log response details");
        }
    }
}