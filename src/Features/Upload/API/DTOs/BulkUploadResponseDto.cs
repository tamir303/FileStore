namespace FileStoreService.Features.Upload.API.DTOs;

public class BulkUploadResponseDto
{
    public List<UploadResponseDto> Results { get; set; } = new();
    public int TotalFiles { get; set; }
    public int SuccessfulUploads { get; set; }
    public int FailedUploads { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    
    public BulkUploadResponseDto() { }
}