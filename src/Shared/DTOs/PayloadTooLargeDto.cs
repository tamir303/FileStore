using System.Net;
using System.Text.Json.Serialization;

namespace FileStoreService.Shared.DTOs;

/// <summary>
/// DTO returned when the request payload exceeds the configured limit (413 Payload Too Large).
/// </summary>
public class PayloadTooLargeDto
{
    /// <summary>
    /// HTTP status code (413).
    /// </summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; } = (int)HttpStatusCode.RequestEntityTooLarge;

    /// <summary>
    /// A brief human‐readable message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Payload too large";

    /// <summary>
    /// The maximum allowed payload size in bytes.
    /// </summary>
    [JsonPropertyName("maxAllowedBytes")]
    public long MaxAllowedBytes { get; set; }

    /// <summary>
    /// The size of the payload that was actually sent in bytes.
    /// </summary>
    [JsonPropertyName("receivedBytes")]
    public long ReceivedBytes { get; set; }

    /// <summary>
    /// UTC timestamp of when the error was generated.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public PayloadTooLargeDto() { }

    public PayloadTooLargeDto(long maxAllowedBytes, long receivedBytes, string? message = null)
    {
        MaxAllowedBytes = maxAllowedBytes;
        ReceivedBytes   = receivedBytes;
        if (!string.IsNullOrWhiteSpace(message))
            Message = message;
    }
}