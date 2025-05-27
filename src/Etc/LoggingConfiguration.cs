using Serilog.Events;
using FileStoreService.Etc.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace FileStoreService.Etc;

public static class LoggingConfiguration
{
    public static IHostBuilder ConfigureLogging(IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
        {
            var loggingSettings = context.Configuration.GetSection("Logging").Get<LoggingSettings>()
                                  ?? new LoggingSettings();

            configuration
                .MinimumLevel.Is(loggingSettings.MinimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            // Console logging
            if (loggingSettings.EnableConsoleLogging)
            {
                if (loggingSettings.EnableStructuredLogging)
                {
                    configuration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
                }
                else
                {
                    configuration.WriteTo.Console(
                        outputTemplate: loggingSettings.ConsoleOutputTemplate);
                }
            }

            // File logging
            if (loggingSettings.EnableFileLogging)
            {
                var logPath = Path.Combine(loggingSettings.LogDirectory, loggingSettings.LogFileName);

                configuration.WriteTo.File(
                    path: logPath,
                    retainedFileCountLimit: loggingSettings.RetainedFileCountLimit,
                    fileSizeLimitBytes: loggingSettings.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: loggingSettings.FileOutputTemplate);
            }

            // Application Insights (if configured)
            if (!string.IsNullOrEmpty(loggingSettings.ApplicationInsightsConnectionString))
            {
                // 1) Create AI telemetry config
                var aiConfig = TelemetryConfiguration.CreateDefault();
                aiConfig.ConnectionString = loggingSettings.ApplicationInsightsConnectionString;

                // 2) Write to AI with a converter (e.g. Traces)
                configuration.WriteTo.ApplicationInsights(
                    aiConfig,
                    TelemetryConverter.Traces
                );
            }

            // Seq logging (if configured)
            if (!string.IsNullOrEmpty(loggingSettings.SeqServerUrl))
            {
                configuration.WriteTo.Seq(
                    serverUrl: loggingSettings.SeqServerUrl,
                    apiKey: loggingSettings.SeqApiKey);
            }
        });
    }
}