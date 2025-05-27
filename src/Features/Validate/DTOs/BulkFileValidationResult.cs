namespace FileStoreService.Features.Validate.DTOs;

/// <summary>
/// Result of bulk file validation
/// </summary>
public class BulkFileValidationResult
{
    public bool IsValid { get; set; }
    public List<FileValidationResult> FileResults { get; set; } = new();
    public List<string> GlobalErrors { get; set; } = new();
    public long TotalSize { get; set; }
    public int ValidFileCount { get; set; }
    public int InvalidFileCount { get; set; }
}