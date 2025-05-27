namespace FileStoreService.Etc.Models;

public class RateLimitingSettings
{
    public bool EnableRateLimiting { get; set; } = true;
    public RateLimitPolicy FileUploadPolicy { get; set; } = new();
    public RateLimitPolicy BulkUploadPolicy { get; set; } = new();
    public RateLimitPolicy FileDownloadPolicy { get; set; } = new();
    public RateLimitPolicy FileSearchPolicy { get; set; } = new();
}