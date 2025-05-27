namespace FileStoreService.Features.Validate.DTOs;

/// <summary>
/// Result of file validation operation
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? SanitizedFileName { get; set; }
    public string? DetectedContentType { get; set; }
    public long FileSize { get; set; }
    public string? Checksum { get; set; }
}