using System.Reflection;
using FileStoreService.Etc.Models;
using FileStoreService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FileStoreService.Etc;

public static class ConfigurationInjection
{
    public static IServiceCollection AddConfigurationOptions(IServiceCollection services, IConfiguration configuration)
    {
        // Bind strongly-typed settings
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));
        services.Configure<RateLimitingSettings>(configuration.GetSection("RateLimiting"));
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));
        services.Configure<SwaggerSettings>(configuration.GetSection("Swagger"));
        services.Configure<LoggingSettings>(configuration.GetSection("Logging"));

        // Register the settings instances
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<FileStorageSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<CacheSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RateLimitingSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SecuritySettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<SwaggerSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LoggingSettings>>().Value);

        return services;
    }

    public static IServiceCollection AddDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>();
        var mongoSettings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();

        // SQL Server
        if (dbSettings?.EnableSqlServer == true)
        {
            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseSqlServer(
                    dbSettings.SqlServerConnectionString,
                    sqlOpts =>
                    {
                        sqlOpts.EnableRetryOnFailure(
                            maxRetryCount: dbSettings.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(dbSettings.MaxRetryDelaySeconds),
                            errorNumbersToAdd: null);
                        sqlOpts.CommandTimeout(dbSettings.CommandTimeoutSeconds);
                        sqlOpts.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    });

                if (dbSettings.EnableDetailedErrors) opts.EnableDetailedErrors();
                if (dbSettings.EnableSensitiveDataLogging) opts.EnableSensitiveDataLogging();
            });
        }

        // MongoDB
        if (mongoSettings?.Enabled == true)
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
                settings.ConnectTimeout = TimeSpan.FromSeconds(mongoSettings.ConnectTimeoutSeconds);
                settings.SocketTimeout = TimeSpan.FromSeconds(mongoSettings.SocketTimeoutSeconds);
                settings.MaxConnectionPoolSize = mongoSettings.MaxConnectionPoolSize;
                settings.MinConnectionPoolSize = mongoSettings.MinConnectionPoolSize;
                return new MongoClient(settings);
            });
            services.AddScoped(sp =>
                sp.GetRequiredService<IMongoClient>()
                  .GetDatabase(mongoSettings.DatabaseName)
            );
            // services.AddScoped<IFileRepository, MongoFileRepository>();
        }

        return services;
    }
}
