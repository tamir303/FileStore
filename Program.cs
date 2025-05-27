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
        opts.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = swaggerSettings.Title,
            Version = swaggerSettings.Version,
            Description = swaggerSettings.Description,
            Contact = new OpenApiContact
            {
                Name = swaggerSettings.ContactName,
                Email = swaggerSettings.ContactEmail,
                Url = string.IsNullOrEmpty(swaggerSettings.ContactUrl) ? null : new Uri(swaggerSettings.ContactUrl)
            }
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

        opts.OperationFilter<FileUploadOperationFilter>();
        opts.SchemaFilter<EnumSchemaFilter>();
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

if (swaggerSettings.EnableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", $"{swaggerSettings.Title} {swaggerSettings.Version}");
        opts.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        opts.DisplayRequestDuration();
        opts.EnableTryItOutByDefault();
        opts.RoutePrefix = "swagger";
    });
}

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