namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of bulk deletion operation
/// </summary>
public class BulkDeletionResult
{
    public bool Success { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}