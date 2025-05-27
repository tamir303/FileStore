using Serilog.Events;

namespace FileStoreService.Etc.Models;

public class LoggingSettings
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public bool EnableStructuredLogging { get; set; } = false;
    public string LogDirectory { get; set; } = "logs";
    public string LogFileName { get; set; } = "application-.log";
    public int RetainedFileCountLimit { get; set; } = 31;
    public long FileSizeLimitBytes { get; set; } = 10485760; // 10MB
    public string ConsoleOutputTemplate { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
    public string FileOutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
    public string ApplicationInsightsConnectionString { get; set; } = string.Empty;
    public string SeqServerUrl { get; set; } = string.Empty;
    public string SeqApiKey { get; set; } = string.Empty;
}