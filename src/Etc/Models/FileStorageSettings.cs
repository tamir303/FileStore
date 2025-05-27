namespace FileStoreService.Etc.Models;

public class FileStorageSettings
{
    public string StorageType { get; set; } = "Local"; // Local, Azure, AWS
    public string BasePath { get; set; } = "uploads";
    public string TempPath { get; set; } = "temp";
    public long MaxFileSizeBytes { get; set; } = 104857600; // 100MB
    public long MaxBulkSizeBytes { get; set; } = 524288000; // 500MB
    public List<string> AllowedContentTypes { get; set; } = new();
    public bool EnableCompression { get; set; } = true;
    public bool EnableVirusScanning { get; set; } = false;
    public string CdnBaseUrl { get; set; } = string.Empty; 
}