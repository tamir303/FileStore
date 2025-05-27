using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using FileStoreService.Etc;
using FileStoreService.Etc.Models;
using FileStoreService.Features.Download.Application.Services;
using FileStoreService.Features.Search.Application.Services;
using FileStoreService.Features.Storage.Application.Services;
using FileStoreService.Features.Upload.Application.Services;
using FileStoreService.Features.Validate.Application.Services;
using FileStoreService.Shared.Constants;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// =======================
// LOGGING CONFIGURATION
// =======================
LoggingConfiguration.ConfigureLogging(builder.Host);

// =======================
// CONFIGURATION SOURCES
// =======================
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// =======================
// ENVIRONMENT CONFIGURATION
// =======================
EnvironmentConfiguration.AddEnvironmentConfiguration(builder.Services, builder.Configuration, builder.Environment);

// =======================
// BIND OPTIONS & DB
// =======================
ConfigurationInjection.AddConfigurationOptions(builder.Services, builder.Configuration);
ConfigurationInjection.AddDatabaseServices(builder.Services, builder.Configuration);

// =======================
// AUTHENTICATION & AUTHORIZATION
// =======================
var securitySettings = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<SecuritySettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = securitySettings.Jwt.ValidateIssuer,
        ValidateAudience = securitySettings.Jwt.ValidateAudience,
        ValidateLifetime = securitySettings.Jwt.ValidateLifetime,
        ValidateIssuerSigningKey = securitySettings.Jwt.ValidateIssuerSigningKey,
        ValidIssuer = securitySettings.Jwt.Issuer,
        ValidAudience = securitySettings.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securitySettings.Jwt.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Log.Warning("JWT Authentication failed: {Error}", ctx.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            Log.Debug("JWT Token validated for user: {User}", ctx.Principal?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy("FileUploadPolicy", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "file:upload"));
    options.AddPolicy("FileDownloadPolicy", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "file:download"));
    options.AddPolicy("FileSearchPolicy", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "file:search"));
});

// =======================
// CORS
// =======================
CorsConfiguration.AddCorsConfiguration(builder.Services, builder.Configuration);

// =======================
// RATE LIMITING
// =======================
var rateLimitingSettings = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<RateLimitingSettings>();

if (rateLimitingSettings.EnableRateLimiting)
{
    builder.Services.AddRateLimiter(options =>
    {
        // File Upload Policy
        options.AddFixedWindowLimiter(FileConstants.RATE_LIMIT_UPLOAD_POLICY, limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitingSettings.FileUploadPolicy.PermitLimit;
            limiterOptions.Window = rateLimitingSettings.FileUploadPolicy.Window;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitingSettings.FileUploadPolicy.QueueLimit;
        });

        // Bulk Upload Policy
        options.AddFixedWindowLimiter(FileConstants.RATE_LIMIT_BULK_UPLOAD_POLICY, limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitingSettings.BulkUploadPolicy.PermitLimit;
            limiterOptions.Window = rateLimitingSettings.BulkUploadPolicy.Window;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitingSettings.BulkUploadPolicy.QueueLimit;
        });

        // File Download Policy
        options.AddFixedWindowLimiter(FileConstants.RATE_LIMIT_DOWNLOAD_POLICY, limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitingSettings.FileDownloadPolicy.PermitLimit;
            limiterOptions.Window = rateLimitingSettings.FileDownloadPolicy.Window;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitingSettings.FileDownloadPolicy.QueueLimit;
        });

        // File Search Policy
        options.AddFixedWindowLimiter(FileConstants.RATE_LIMIT_SEARCH_POLICY, limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitingSettings.FileSearchPolicy.PermitLimit;
            limiterOptions.Window = rateLimitingSettings.FileSearchPolicy.Window;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitingSettings.FileSearchPolicy.QueueLimit;
        });

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1)
                }));

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new 
                    {
                        error = "Rate limit exceeded", 
                        retryAfter = retryAfter.TotalSeconds
                    }), 
                    cancellationToken: token);
            }
        };
    });
}

// =======================
// CACHING
// =======================
var cacheSettings = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<CacheSettings>();

if (cacheSettings.EnableMemoryCache)
    builder.Services.AddMemoryCache(opts => opts.SizeLimit = cacheSettings.MemoryCacheSizeLimitBytes);

if (cacheSettings.EnableRedisCache && !string.IsNullOrEmpty(cacheSettings.RedisConnectionString))
    builder.Services.AddStackExchangeRedisCache(opts =>
        opts.Configuration = cacheSettings.RedisConnectionString);

// =======================
// CONTROLLERS & JSON
// =======================
builder.Services.AddControllers(options =>
    options.SuppressAsyncSuffixInActionNames = false)
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// =======================
// APPLICATION SERVICES
// =======================
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddScoped<IFileSearchService, FileSearchService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// HTTP context & clients
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// =======================
// SWAGGER & OPENAPI
// =======================
var swaggerSettings = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<SwaggerSettings>();

if (swaggerSettings.EnableSwagger)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        // Basic API Info with enhanced description
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        
        opts.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = swaggerSettings.Title,
            Version = swaggerSettings.Version,
            Description = $"## \ud83d\ude80 {swaggerSettings.Description}\n\n**Build Version:** `{version}`  \n**Environment:** `{builder.Environment.EnvironmentName}`  \n**Framework:** `.NET {Environment.Version}`\n\n### \ud83c\udfaf Key Features\n- **\ud83d\udce4 File Upload**: Single and bulk file uploads with comprehensive validation\n- **\ud83d\udce5 File Download**: Direct downloads with resume support and bulk archive creation  \n- **\ud83d\udd0d Advanced Search**: Full-text search with filters, faceting, and suggestions\n- **\ud83d\udd10 Security**: JWT-based authentication with role-based access control\n- **\u26a1 Rate Limiting**: Configurable protection against abuse\n- **\ud83d\udcca Monitoring**: Health checks, metrics, and comprehensive logging\n\n### \ud83d\udd11 Authentication\nThis API uses **JWT Bearer tokens**. Include your token in the Authorization header:\n```\nAuthorization: Bearer <your-jwt-token>\n```\n\n### \ud83d\udcc8 Rate Limits\n- **File Upload**: {rateLimitingSettings.FileUploadPolicy.PermitLimit} requests per minute\n- **Bulk Upload**: {rateLimitingSettings.BulkUploadPolicy.PermitLimit} requests per {rateLimitingSettings.BulkUploadPolicy.Window.TotalMinutes} minutes\n- **File Download**: {rateLimitingSettings.FileDownloadPolicy.PermitLimit} requests per minute  \n- **Search Operations**: {rateLimitingSettings.FileSearchPolicy.PermitLimit} requests per minute\n\n### \ud83d\udcde Support & Documentation.",
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            },
        });

        if (swaggerSettings.EnableJwtBearer)
        {
            opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Bearer token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }

        if (swaggerSettings.EnableXmlComments)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                opts.IncludeXmlComments(xmlPath);
        }

        // Add all custom filters with error handling
        try
        {
            opts.OperationFilter<FileUploadOperationFilter>();
            opts.OperationFilter<CorrelationIdOperationFilter>();
            opts.OperationFilter<RateLimitingOperationFilter>();
            opts.OperationFilter<AuthResponseOperationFilter>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register some operation filters");
        }
        
        try
        {
            opts.SchemaFilter<EnumSchemaFilter>();
            opts.SchemaFilter<ExampleSchemaFilter>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register some schema filters");
        }

        try
        {
            opts.DocumentFilter<SecurityRequirementsDocumentFilter>();
            opts.DocumentFilter<TagOrderDocumentFilter>();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to register some document filters");
        }

        // Configure serialization
        opts.UseAllOfToExtendReferenceSchemas();
        opts.UseOneOfForPolymorphism();

        // Configure operation sorting and grouping
        opts.OrderActionsBy(apiDesc =>
            $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}_{apiDesc.RelativePath}");

        if (builder.Environment.IsDevelopment())
        {
            opts.AddServer(new OpenApiServer
            {
                Url = "https://localhost:5001",
                Description = "Development Server (HTTPS)"
            });
            opts.AddServer(new OpenApiServer
            {
                Url = "http://localhost:5000",
                Description = "Development Server (HTTP)"
            });
        }
    });
}

// =======================
// HEALTH CHECKS
// =======================
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql-server",
        tags: new[] { "database", "sql" });

var app = builder.Build();

// =======================
// MIDDLEWARE PIPELINE
// =======================
if (securitySettings.RequireHttps)
    app.UseHttpsRedirection();

if (securitySettings.EnableHsts && !builder.Environment.IsDevelopment())
    app.UseHsts();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Use the correct CORS middleware method
app.UseCors(CorsConfiguration.DEFAULT_POLICY_NAME);

if (rateLimitingSettings.EnableRateLimiting)
    app.UseRateLimiter();

app.UseStaticFiles();
app.UseRouting();

// Enhanced Swagger UI Configuration
if (swaggerSettings.EnableSwagger)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            // Add runtime server information
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer 
                { 
                    Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
                    Description = $"Current Server ({builder.Environment.EnvironmentName})"
                }
            };
        });
    });

    app.UseSwaggerUI(opts =>
    {
        // Basic configuration
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", $"{swaggerSettings.Title} v1");
        
        // Enhanced UI settings
        opts.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        opts.DisplayRequestDuration();
        opts.EnableTryItOutByDefault();
        opts.EnableDeepLinking();
        opts.EnableFilter();
        opts.ShowExtensions();
        opts.EnableValidator();
        opts.SupportedSubmitMethods(
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, 
            Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete);
        
        // Try to inject custom files if they exist
        var customCssPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "swagger-ui", "custom.css");
        var customJsPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "swagger-ui", "custom.js");
        
        if (File.Exists(customCssPath))
        {
            opts.InjectStylesheet("/swagger-ui/custom.css");
        }
        
        if (File.Exists(customJsPath))
        {
            opts.InjectJavascript("/swagger-ui/custom.js");
        }
        
        // OAuth configuration (if needed)
        opts.OAuthClientId("swagger-ui");
        opts.OAuthAppName("File Management API");
        opts.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        
        // Custom HTML head content
        opts.HeadContent = @"
            <meta name='description' content='File Management API - Upload, download, and search files with enterprise features'>
            <meta name='keywords' content='API, File Management, Upload, Download, Search, REST'>
            <meta name='author' content='Development Team'>
            <link rel='icon' type='image/png' href='/favicon.ico'>
            <style>
                .swagger-ui .topbar { min-height: 60px; }
                body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
            </style>
        ";
        
        // Route prefix
        opts.RoutePrefix = "swagger";
        
        // Enhanced configuration
        opts.ConfigObject.AdditionalItems["tryItOutEnabled"] = true;
        opts.ConfigObject.AdditionalItems["filter"] = true;
        opts.ConfigObject.AdditionalItems["persistAuthorization"] = true;
        opts.ConfigObject.AdditionalItems["displayOperationId"] = false;
        opts.ConfigObject.AdditionalItems["defaultModelsExpandDepth"] = 2;
        opts.ConfigObject.AdditionalItems["defaultModelExpandDepth"] = 3;
        opts.ConfigObject.AdditionalItems["showRequestHeaders"] = true;
        opts.ConfigObject.AdditionalItems["showCommonExtensions"] = true;
        opts.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["activated"] = true,
            ["theme"] = "agate"
        };
    });
}

// Serve custom Swagger UI assets if they exist
var swaggerUiPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "swagger-ui");
if (Directory.Exists(swaggerUiPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(swaggerUiPath),
        RequestPath = "/swagger-ui"
    });
}

// Add API info endpoint
app.MapGet("/api/info", () => new
{
    Title = swaggerSettings.Title,
    Version = swaggerSettings.Version,
    Environment = builder.Environment.EnvironmentName,
    BuildDate = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location),
    Framework = $".NET {Environment.Version}",
    Features = new[]
    {
        "JWT Authentication",
        "Rate Limiting", 
        "File Upload/Download",
        "Advanced Search",
        "Health Monitoring",
        "Request Correlation",
        "Comprehensive Logging"
    },
    Endpoints = new
    {
        Swagger = "/swagger",
        Health = "/health",
        Upload = "/api/v1/upload",
        Download = "/api/v1/download", 
        Search = "/api/v1/search"
    },
    RateLimits = new
    {
        Upload = $"{rateLimitingSettings.FileUploadPolicy.PermitLimit} per {rateLimitingSettings.FileUploadPolicy.Window.TotalMinutes}min",
        BulkUpload = $"{rateLimitingSettings.BulkUploadPolicy.PermitLimit} per {rateLimitingSettings.BulkUploadPolicy.Window.TotalMinutes}min",
        Download = $"{rateLimitingSettings.FileDownloadPolicy.PermitLimit} per {rateLimitingSettings.FileDownloadPolicy.Window.TotalMinutes}min",
        Search = $"{rateLimitingSettings.FileSearchPolicy.PermitLimit} per {rateLimitingSettings.FileSearchPolicy.Window.TotalMinutes}min"
    }
})
.WithName("GetApiInfo")
.WithTags("Information")
.Produces<object>(200);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Database migration on startup
if (builder.Environment.IsDevelopment() || args.Contains("--migrate"))
{
    await app.MigrateDatabaseAsync();
}

Log.Information("File Management API started on {Env}", builder.Environment.EnvironmentName);
await app.RunAsync();