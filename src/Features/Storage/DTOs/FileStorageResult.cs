namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of file storage operation
/// </summary>
public class FileStorageResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public long FileSize { get; set; }
    public string? Checksum { get; set; }
    public DateTime StoredAt { get; set; }
    public Dictionary<string, string>? StorageMetadata { get; set; }
}