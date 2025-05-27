namespace FileStoreService.Features.Validate.DTOs;

/// <summary>
/// Result of file name validation
/// </summary>
public class FileNameValidationResult
{
    public bool IsValid { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string SanitizedFileName { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
}