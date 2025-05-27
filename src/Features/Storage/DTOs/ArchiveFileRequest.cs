namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Request for including a file in an archive
/// </summary>
public class ArchiveFileRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string? RelativePath { get; set; }
}