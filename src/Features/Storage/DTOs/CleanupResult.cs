namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of cleanup operation
/// </summary>
public class CleanupResult
{
    public bool Success { get; set; }
    public int FilesRemoved { get; set; }
    public long SpaceFreedBytes { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}