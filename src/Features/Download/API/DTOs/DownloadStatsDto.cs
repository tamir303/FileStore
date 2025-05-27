namespace FileStoreService.Features.Download.API.DTOs;

public class DownloadStatsDto
{
    public string FileId { get; set; } = string.Empty;
    public int DownloadCount { get; set; }
    public DateTime LastDownloaded { get; set; }
    public List<DownloadLogDto> RecentDownloads { get; set; } = new();
    
    public DownloadStatsDto() { }
}