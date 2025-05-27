namespace FileStoreService.Etc.Models;

public class SecuritySettings
{
    public bool RequireHttps { get; set; } = true;
    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAgeSeconds { get; set; } = 31536000; // 1 year
    public bool EnableAntiforgery { get; set; } = true;
    public string[] AllowedHosts { get; set; } = { "*" };
    public JwtSettings Jwt { get; set; } = new();
}