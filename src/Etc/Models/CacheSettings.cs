namespace FileStoreService.Etc.Models;

public class CacheSettings
{
    public bool EnableMemoryCache { get; set; } = true;
    public bool EnableRedisCache { get; set; } = false;
    public string RedisConnectionString { get; set; } = string.Empty;
    public int DefaultCacheDurationMinutes { get; set; } = 30;
    public long MemoryCacheSizeLimitBytes { get; set; } = 104857600; // 100MB
}