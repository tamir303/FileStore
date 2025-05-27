namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Result of file retrieval operation
/// </summary>
public class FileRetrievalResult
{
    public bool Success { get; set; }
    public Stream? FileStream { get; set; }
    public FileMetadata? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}