using Microsoft.AspNetCore.Cors.Infrastructure;

namespace FileStoreService.Etc.Models;

public class CorsSettings
{
    public CorsPolicy DefaultPolicy { get; set; } = new();
    public CorsPolicy StrictPolicy { get; set; } = new();
}