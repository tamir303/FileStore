namespace FileStoreService.Features.Upload.API.DTOs;

public class UploadResponseDto
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public UploadResponseDto() { }
}

