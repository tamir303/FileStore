namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// Storage usage statistics
/// </summary>
public class StorageUsageStats
{
    public long TotalSizeBytes { get; set; }
    public int TotalFileCount { get; set; }
    public long AvailableSpaceBytes { get; set; }
    public Dictionary<string, long> SizeByContentType { get; set; } = new();
    public Dictionary<string, int> CountByContentType { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}