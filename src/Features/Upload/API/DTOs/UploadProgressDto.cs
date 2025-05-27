using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Upload.API.DTOs;

public class UploadProgressDto
{
    public string UploadId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
    public double PercentageComplete { get; set; }
    public UploadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastUpdated { get; set; } 
    
    public UploadProgressDto() { }
}