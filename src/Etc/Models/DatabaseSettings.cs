namespace FileStoreService.Etc.Models;

public class DatabaseSettings
{
    public bool EnableSqlServer { get; set; } = true;
    public string SqlServerConnectionString { get; set; } = string.Empty;
    public bool EnableAutoMigration { get; set; } = true;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelaySeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 30;
}