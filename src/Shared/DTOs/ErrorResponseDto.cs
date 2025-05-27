using System.Net;
using System.Text.Json.Serialization;

namespace FileStoreService.Shared.DTOs;
/// <summary>
/// Standard DTO for returning error information from the API.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// HTTP status code of the error.
    /// </summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>
    /// A short, human-readable summary of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
    
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error information (stack trace, inner exception, etc.) – omit or conditionally include in production.
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// A list of validation or domain‐specific error messages.
    /// </summary>
    [JsonPropertyName("errors")]
    public IEnumerable<string>? Errors { get; set; }

    /// <summary>
    /// UTC timestamp of when the error occurred.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public ErrorResponseDto() { }

    public ErrorResponseDto(
        HttpStatusCode statusCode,
        string message,
        string? detail = null,
        IEnumerable<string>? errors = null)
    {
        StatusCode  = (int)statusCode;
        Message     = message;
        Detail      = detail;
        Errors      = errors;
        Timestamp   = DateTimeOffset.UtcNow;
    }
}