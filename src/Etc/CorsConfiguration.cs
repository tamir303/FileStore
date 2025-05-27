using FileStoreService.Etc.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using CorsPolicy = FileStoreService.Etc.Models.CorsPolicy;

public class CorsConfiguration
{
    public const string DEFAULT_POLICY_NAME = "DefaultCorsPolicy";
    public const string STRICT_POLICY_NAME = "StrictCorsPolicy";
    public const string DEVELOPMENT_POLICY_NAME = "DevelopmentCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            // Default policy - configurable via appsettings
            options.AddPolicy(DEFAULT_POLICY_NAME, policy =>
            {
                ConfigurePolicy(policy, corsSettings.DefaultPolicy);
            });

            // Strict policy for production
            options.AddPolicy(STRICT_POLICY_NAME, policy =>
            {
                policy
                    .WithOrigins(corsSettings.StrictPolicy.AllowedOrigins.ToArray())
                    .WithMethods(corsSettings.StrictPolicy.AllowedMethods.ToArray())
                    .WithHeaders(corsSettings.StrictPolicy.AllowedHeaders.ToArray())
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(corsSettings.StrictPolicy.PreflightMaxAgeMinutes));

                if (corsSettings.StrictPolicy.AllowCredentials)
                    policy.AllowCredentials();
            });

            // Development policy - more permissive
            options.AddPolicy(DEVELOPMENT_POLICY_NAME, policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    private static void ConfigurePolicy(CorsPolicyBuilder policy, CorsPolicy corsPolicy)
    {
        // Origins
        if (corsPolicy.AllowAnyOrigin)
            policy.AllowAnyOrigin();
        else if (corsPolicy.AllowedOrigins.Any())
            policy.WithOrigins(corsPolicy.AllowedOrigins.ToArray());

        // Methods
        if (corsPolicy.AllowAnyMethod)
            policy.AllowAnyMethod();
        else if (corsPolicy.AllowedMethods.Any())
            policy.WithMethods(corsPolicy.AllowedMethods.ToArray());

        // Headers
        if (corsPolicy.AllowAnyHeader)
            policy.AllowAnyHeader();
        else if (corsPolicy.AllowedHeaders.Any())
            policy.WithHeaders(corsPolicy.AllowedHeaders.ToArray());

        // Credentials
        if (corsPolicy.AllowCredentials)
            policy.AllowCredentials();

        // Preflight max age
        if (corsPolicy.PreflightMaxAgeMinutes > 0)
            policy.SetPreflightMaxAge(TimeSpan.FromMinutes(corsPolicy.PreflightMaxAgeMinutes));

        // Exposed headers
        if (corsPolicy.ExposedHeaders.Any())
            policy.WithExposedHeaders(corsPolicy.ExposedHeaders.ToArray());
    }
}