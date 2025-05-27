namespace FileStoreService.Etc.Models;

public class RateLimitPolicy
{
    public int PermitLimit { get; set; } = 10;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int QueueLimit { get; set; } = 5;
}