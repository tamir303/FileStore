using FileStoreService.Etc.Models;
using Microsoft.AspNetCore.Hosting;
using Serilog.Events;

namespace FileStoreService.Etc;

public static class EnvironmentConfiguration
{
    public static IServiceCollection AddEnvironmentConfiguration(IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Environment-specific configurations
        if (environment.IsDevelopment())
        {
            AddDevelopmentServices(services, configuration);
        }
        else if (environment.IsStaging())
        {
            AddStagingServices(services, configuration);
        }
        else if (environment.IsProduction())
        { 
            AddProductionServices(services, configuration);
        }

        // Common environment settings
        services.Configure<EnvironmentSettings>(options =>
        {
            options.EnvironmentName = environment.EnvironmentName;
            options.ApplicationName = environment.ApplicationName;
            options.ContentRootPath = environment.ContentRootPath;
            options.WebRootPath = environment.WebRootPath;
            options.IsDevelopment = environment.IsDevelopment();
            options.IsStaging = environment.IsStaging();
            options.IsProduction = environment.IsProduction();
        });

        return services;
    }

    private static IServiceCollection AddDevelopmentServices(IServiceCollection services,
        IConfiguration configuration)
    {
        // More verbose logging in development
        services.Configure<LoggingSettings>(options =>
        {
            options.MinimumLevel = LogEventLevel.Debug;
            options.EnableConsoleLogging = true;
            options.EnableFileLogging = true;
        });

        return services;
    }

    private static IServiceCollection AddStagingServices(IServiceCollection services, IConfiguration configuration)
    {
        // Staging-specific services
        services.Configure<LoggingSettings>(options =>
        {
            options.MinimumLevel = LogEventLevel.Information;
            options.EnableConsoleLogging = true;
            options.EnableFileLogging = true;
        });

        return services;
    }

    private static IServiceCollection AddProductionServices(IServiceCollection services,
        IConfiguration configuration)
    {
        // Production-specific services
        services.Configure<LoggingSettings>(options =>
        {
            options.MinimumLevel = LogEventLevel.Warning;
            options.EnableConsoleLogging = false;
            options.EnableFileLogging = true;
            options.EnableStructuredLogging = true;
        });

        return services;
    }
}