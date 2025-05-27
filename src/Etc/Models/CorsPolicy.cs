namespace FileStoreService.Etc.Models;

public class CorsPolicy
{
    public bool AllowAnyOrigin { get; set; } = false;
    public bool AllowAnyMethod { get; set; } = false;
    public bool AllowAnyHeader { get; set; } = false;
    public bool AllowCredentials { get; set; } = false;
    public List<string> AllowedOrigins { get; set; } = new();
    public List<string> AllowedMethods { get; set; } = new();
    public List<string> AllowedHeaders { get; set; } = new();
    public List<string> ExposedHeaders { get; set; } = new();
    public int PreflightMaxAgeMinutes { get; set; } = 30;
}