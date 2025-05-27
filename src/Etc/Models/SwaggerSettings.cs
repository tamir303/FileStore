namespace FileStoreService.Etc.Models;

public class SwaggerSettings
{
    public bool EnableSwagger { get; set; } = true;
    public string Title { get; set; } = "File Management API";
    public string Description { get; set; } = "API for file upload, download, and search operations";
    public string Version { get; set; } = "v1";
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactUrl { get; set; } = string.Empty;
    public bool EnableXmlComments { get; set; } = true;
    public bool EnableJwtBearer { get; set; } = true;
    public List<string> JsonIgnoreProperties { get; set; } = new();
}