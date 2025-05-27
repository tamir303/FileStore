using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Shared.Extensions;

public static class HttpContextExtensions
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    
    /// <summary>
    /// Gets or generates a correlation ID for the current request.
    /// </summary>
    public static string? GetCorrelationId(this HttpContext ctx)
    {
        // Prefer an incoming header if present
        if (ctx.Request.Headers.TryGetValue(CorrelationIdHeader, out var values) &&
            !string.IsNullOrWhiteSpace(values.First()))
        {
            return values.First();
        }

        // Otherwise generate a new one
        var newId = Guid.NewGuid().ToString("N");
        ctx.Response.Headers[CorrelationIdHeader] = newId;
        return newId;
    }

    /// <summary>
    /// Attempts to resolve the client IP (X-Forwarded-For fallback).
    /// </summary>
    public static string GetClientIpAddress(this HttpContext ctx)
    {
        // If behind proxy, use the X-Forwarded-For header
        if (!ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ip = forwarded.First()?.Split(',').FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ip))
            return ip.Trim();

        // Fallback to remote IP
        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public static class ClaimsPrincipalExtensions
{
    private const string ClaimUserId = ClaimTypes.NameIdentifier;

    /// <summary>
    /// Reads the user ID from the JWT (NameIdentifier claim).
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var claim = user.FindFirst(ClaimUserId)
                    ?? throw new InvalidOperationException("User ID claim not found");

        return claim.Value;
    }
}