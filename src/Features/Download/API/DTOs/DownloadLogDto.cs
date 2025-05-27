namespace FileStoreService.Features.Download.API.DTOs;

public class DownloadLogDto
{
    public DateTime DownloadedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? UserId { get; set; }
    public long BytesTransferred { get; set; }
    public bool Completed { get; set; }

    public DownloadLogDto() { }
}