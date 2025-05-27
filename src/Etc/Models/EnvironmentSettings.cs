namespace FileStoreService.Etc.Models;

public class EnvironmentSettings
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;
    public string WebRootPath { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; }
    public bool IsStaging { get; set; }
    public bool IsProduction { get; set; }
}