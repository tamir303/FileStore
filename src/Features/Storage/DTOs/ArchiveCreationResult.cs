namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of archive creation
/// </summary>
public class ArchiveCreationResult
{
    public bool Success { get; set; }
    public Stream? ArchiveStream { get; set; }
    public long ArchiveSize { get; set; }
    public string? ErrorMessage { get; set; }
    public int FilesIncluded { get; set; }
    public List<string> FailedFiles { get; set; } = new();
}