namespace FileStoreService.Etc.Models;

public class MongoDbSettings
{
    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "FileManagement";
    public int ConnectTimeoutSeconds { get; set; } = 30;
    public int SocketTimeoutSeconds { get; set; } = 30;
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 5;
}