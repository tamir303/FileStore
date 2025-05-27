namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of bulk storage operation
/// </summary>
public class BulkStorageResult
{
    public bool Success { get; set; }
    public List<FileStorageResult> Results { get; set; } = new();
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}