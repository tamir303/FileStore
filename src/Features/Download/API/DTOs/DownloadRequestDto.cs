using System.ComponentModel.DataAnnotations;

namespace FileStoreService.Features.Download.API.DTOs;

public class DownloadRequestDto
{
    [Required]
    public string FileId { get; set; } = string.Empty;
        
    public bool ForceDownload { get; set; } = false;
        
    public string? CustomFileName { get; set; }
        
    // For partial downloads
    public long? RangeStart { get; set; }
    public long? RangeEnd { get; set; }
    
    public DownloadRequestDto() { }
}