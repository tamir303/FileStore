namespace FileStoreService.Features.Validate.DTOs;

/// <summary>
/// Result of metadata validation
/// </summary>
public class MetadataValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> SanitizedMetadata { get; set; } = new();
}