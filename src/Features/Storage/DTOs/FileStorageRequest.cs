namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Request for storing a file
/// </summary>
public class FileStorageRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
}