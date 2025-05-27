using FileStoreService.Etc.Models;
using FileStoreService.Infrastructure.Context;
using FileStoreService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace FileStoreService.Etc;
    
public static class DatabaseMigrationHelper
{
    public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var databaseSettings = services.GetService<DatabaseSettings>();

            if (databaseSettings?.EnableAutoMigration == true)
            {
                await MigrateSqlServerAsync(services, logger);
            }

            await InitializeMongoDbAsync(services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }

        return host;
    }

    private static async Task MigrateSqlServerAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var context = services.GetService<ApplicationDbContext>();
            if (context != null)
            {
                logger.LogInformation("Starting SQL Server database migration...");

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                        pendingMigrations.Count(), string.Join(", ", pendingMigrations));

                    await context.Database.MigrateAsync();
                    logger.LogInformation("SQL Server database migration completed successfully");
                }
                else
                {
                    logger.LogInformation("No pending SQL Server migrations found");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to migrate SQL Server database");
            throw;
        }
    }

    private static async Task InitializeMongoDbAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var mongoSettings = services.GetService<MongoDbSettings>();
            if (mongoSettings?.Enabled == true)
            {
                logger.LogInformation("Initializing MongoDB...");

                var database = services.GetService<IMongoDatabase>();
                if (database != null)
                {
                    // Create indexes for file collection
                    var filesCollection = database.GetCollection<FileDocument>("files");

                    var indexKeysDefinition = Builders<FileDocument>.IndexKeys
                        .Ascending(f => f.FileName)
                        .Ascending(f => f.UploadedBy)
                        .Ascending(f => f.Category)
                        .Ascending(f => f.UploadedAt);

                    var indexOptions = new CreateIndexOptions { Background = true };
                    var indexModel = new CreateIndexModel<FileDocument>(indexKeysDefinition, indexOptions);

                    await filesCollection.Indexes.CreateOneAsync(indexModel);

                    // Create text index for search
                    var textIndexKeys = Builders<FileDocument>.IndexKeys
                        .Text(f => f.FileName)
                        .Text(f => f.Description)
                        .Text(f => f.Tags);

                    var textIndexModel = new CreateIndexModel<FileDocument>(textIndexKeys,
                        new CreateIndexOptions { Background = true, Name = "text_search_index" });

                    await filesCollection.Indexes.CreateOneAsync(textIndexModel);

                    logger.LogInformation("MongoDB initialization completed successfully");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize MongoDB");
            throw;
        }
    }
}