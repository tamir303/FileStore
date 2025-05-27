namespace FileStoreService.Features.Download.API.DTOs;

public class BulkDownloadResponseDto
{
    public string ArchiveId { get; set; } = string.Empty;
    public string ArchiveName { get; set; } = string.Empty;
    public long ArchiveSize { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int TotalFiles { get; set; }
    public List<string> IncludedFiles { get; set; } = new();
    public List<string> FailedFiles { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    public BulkDownloadResponseDto() { }
}