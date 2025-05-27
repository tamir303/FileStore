using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;

namespace FileStoreService.Etc.Models;

/// <summary>
/// Adds correlation ID header to all operations
/// </summary>
public class CorrelationIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add correlation ID to response headers
        foreach (var response in operation.Responses.Values)
        {
            response.Headers ??= new Dictionary<string, OpenApiHeader>();
            response.Headers["X-Correlation-ID"] = new OpenApiHeader
            {
                Description = "Unique identifier for tracking this request across services",
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
            };
        }

        // Add correlation ID as optional request header
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Optional correlation ID for request tracking. If not provided, one will be generated.",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        });
    }
}

/// <summary>
/// Adds rate limiting information to operations
/// </summary>
public class RateLimitingOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var rateLimitAttribute = context.MethodInfo
            .GetCustomAttributes<EnableRateLimitingAttribute>()
            .FirstOrDefault();

        if (rateLimitAttribute != null)
        {
            operation.Description += $"\n\n**Rate Limiting**: This endpoint is protected by rate limiting policy: `{rateLimitAttribute.PolicyName}`";
            
            // Add rate limit headers to responses
            foreach (var response in operation.Responses.Values)
            {
                response.Headers ??= new Dictionary<string, OpenApiHeader>();
                response.Headers["X-RateLimit-Remaining"] = new OpenApiHeader
                {
                    Description = "Number of requests remaining in the current window",
                    Schema = new OpenApiSchema { Type = "integer" }
                };
                response.Headers["X-RateLimit-Reset"] = new OpenApiHeader
                {
                    Description = "UTC date/time when the rate limit window resets",
                    Schema = new OpenApiSchema { Type = "string", Format = "date-time" }
                };
            }

            // Add 429 response if not already present
            if (!operation.Responses.ContainsKey("429"))
            {
                operation.Responses["429"] = new OpenApiResponse
                {
                    Description = "Too Many Requests - Rate limit exceeded",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ErrorResponseDto"
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}

/// <summary>
/// Adds authentication response codes and descriptions
/// </summary>
public class AuthResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorizeAttribute = context.MethodInfo
            .GetCustomAttributes<AuthorizeAttribute>()
            .Any() || context.MethodInfo.DeclaringType?
            .GetCustomAttributes<AuthorizeAttribute>()
            .Any() == true;

        if (hasAuthorizeAttribute)
        {
            // Add 401 response
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses["401"] = new OpenApiResponse
                {
                    Description = "Unauthorized - Valid JWT token required",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ErrorResponseDto"
                                }
                            }
                        }
                    }
                };
            }

            // Add 403 response for role-based operations
            var authorizeAttr = context.MethodInfo
                .GetCustomAttributes<AuthorizeAttribute>()
                .FirstOrDefault(a => !string.IsNullOrEmpty(a.Policy));

            if (authorizeAttr != null && !operation.Responses.ContainsKey("403"))
            {
                operation.Responses["403"] = new OpenApiResponse
                {
                    Description = $"Forbidden - Insufficient permissions. Required policy: {authorizeAttr.Policy}",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ErrorResponseDto"
                                }
                            }
                        }
                    }
                };
            }

            operation.Security ??= new List<OpenApiSecurityRequirement>();
        }
    }
}

/// <summary>
/// Adds example values to schemas
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(DateTime) || context.Type == typeof(DateTime?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString("2024-01-15T10:30:00Z");
        }
        else if (context.Type == typeof(Guid) || context.Type == typeof(Guid?))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString("123e4567-e89b-12d3-a456-426614174000");
        }
        else if (context.Type.Name.Contains("RequestDto"))
        {
            AddRequestExamples(schema, context.Type);
        }
        else if (context.Type.Name.Contains("ResponseDto"))
        {
            AddResponseExamples(schema, context.Type);
        }
    }

    private static void AddRequestExamples(OpenApiSchema schema, Type type)
    {
        schema.Example = type.Name switch
        {
            "UploadRequestDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["description"] = new Microsoft.OpenApi.Any.OpenApiString("My important document"),
                ["category"] = new Microsoft.OpenApi.Any.OpenApiString("Documents"),
                ["tags"] = new Microsoft.OpenApi.Any.OpenApiArray
                {
                    new Microsoft.OpenApi.Any.OpenApiString("important"),
                    new Microsoft.OpenApi.Any.OpenApiString("work")
                },
                ["isPublic"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false)
            },
            "SearchRequestDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["query"] = new Microsoft.OpenApi.Any.OpenApiString("financial report"),
                ["contentTypes"] = new Microsoft.OpenApi.Any.OpenApiArray
                {
                    new Microsoft.OpenApi.Any.OpenApiString("application/pdf")
                },
                ["page"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["pageSize"] = new Microsoft.OpenApi.Any.OpenApiInteger(20)
            },
            _ => null
        };
    }

    private static void AddResponseExamples(OpenApiSchema schema, Type type)
    {
        schema.Example = type.Name switch
        {
            "UploadResponseDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["fileId"] = new Microsoft.OpenApi.Any.OpenApiString("123e4567-e89b-12d3-a456-426614174000"),
                ["fileName"] = new Microsoft.OpenApi.Any.OpenApiString("document_20240115_103000.pdf"),
                ["originalFileName"] = new Microsoft.OpenApi.Any.OpenApiString("Financial Report.pdf"),
                ["fileSize"] = new Microsoft.OpenApi.Any.OpenApiLong(2048576),
                ["contentType"] = new Microsoft.OpenApi.Any.OpenApiString("application/pdf"),
                ["uploadedAt"] = new Microsoft.OpenApi.Any.OpenApiString("2024-01-15T10:30:00Z"),
                ["success"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
            },
            _ => null
        };
    }
}

/// <summary>
/// Adds global security requirements and common response schemas
/// </summary>
public class SecurityRequirementsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Ensure Components exists
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

        // Add ErrorResponseDto schema only if it doesn't exist
        if (!swaggerDoc.Components.Schemas.ContainsKey("ErrorResponseDto"))
        {
            swaggerDoc.Components.Schemas.Add("ErrorResponseDto", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["statusCode"] = new OpenApiSchema { Type = "integer", Description = "HTTP status code" },
                    ["message"] = new OpenApiSchema { Type = "string", Description = "Error message" },
                    ["correlationId"] = new OpenApiSchema { Type = "string", Description = "Request correlation ID" },
                    ["detail"] = new OpenApiSchema { Type = "string", Description = "Detailed error information", Nullable = true },
                    ["errors"] = new OpenApiSchema 
                    { 
                        Type = "array", 
                        Items = new OpenApiSchema { Type = "string" },
                        Description = "List of validation errors",
                        Nullable = true
                    },
                    ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "Error timestamp" }
                },
                Required = new HashSet<string> { "statusCode", "message", "correlationId", "timestamp" }
            });
        }

        // Add RateLimitResponse schema only if it doesn't exist
        if (!swaggerDoc.Components.Schemas.ContainsKey("RateLimitResponse"))
        {
            swaggerDoc.Components.Schemas.Add("RateLimitResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["error"] = new OpenApiSchema { Type = "string", Description = "Rate limit error message" },
                    ["retryAfter"] = new OpenApiSchema { Type = "number", Description = "Seconds until rate limit resets" }
                }
            });
        }

        // Add PayloadTooLargeDto schema only if it doesn't exist
        if (!swaggerDoc.Components.Schemas.ContainsKey("PayloadTooLargeDto"))
        {
            swaggerDoc.Components.Schemas.Add("PayloadTooLargeDto", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["statusCode"] = new OpenApiSchema { Type = "integer", Description = "HTTP status code (413)" },
                    ["message"] = new OpenApiSchema { Type = "string", Description = "Error message" },
                    ["maxAllowedBytes"] = new OpenApiSchema { Type = "integer", Format = "int64", Description = "Maximum allowed payload size in bytes" },
                    ["receivedBytes"] = new OpenApiSchema { Type = "integer", Format = "int64", Description = "Size of the payload that was sent" },
                    ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "Error timestamp" }
                },
                Required = new HashSet<string> { "statusCode", "message", "maxAllowedBytes", "receivedBytes", "timestamp" }
            });
        }

        // Enhance existing ErrorResponseDto schema if it was auto-generated
        if (swaggerDoc.Components.Schemas.TryGetValue("ErrorResponseDto", out var existingErrorSchema))
        {
            // Add description if missing
            if (string.IsNullOrEmpty(existingErrorSchema.Description))
            {
                existingErrorSchema.Description = "Standard error response format used across all API endpoints";
            }

            // Ensure all expected properties have descriptions
            if (existingErrorSchema.Properties != null)
            {
                if (existingErrorSchema.Properties.TryGetValue("statusCode", out var statusCodeProp))
                    statusCodeProp.Description ??= "HTTP status code";
                if (existingErrorSchema.Properties.TryGetValue("message", out var messageProp))
                    messageProp.Description ??= "Human-readable error message";
                if (existingErrorSchema.Properties.TryGetValue("correlationId", out var correlationProp))
                    correlationProp.Description ??= "Unique identifier for tracking this request";
            }
        }

        // Add global security schemes documentation
        if (swaggerDoc.Components.SecuritySchemes?.ContainsKey("Bearer") == true)
        {
            swaggerDoc.Info.Description += @"

### 🔐 Authentication Guide

1. **Obtain JWT Token**: Contact your administrator or use the authentication endpoint
2. **Add to Headers**: Include `Authorization: Bearer <token>` in all requests
3. **Token Expiration**: Tokens expire after 60 minutes by default
4. **Refresh Strategy**: Implement token refresh logic in your client application

### 📋 Common Response Codes

| Code | Description | Schema |
|------|-------------|--------|
| 200  | Success | Operation-specific response |
| 400  | Bad Request - Invalid input data | `ErrorResponseDto` |
| 401  | Unauthorized - Missing or invalid JWT token | `ErrorResponseDto` |
| 403  | Forbidden - Insufficient permissions | `ErrorResponseDto` |
| 413  | Payload Too Large - File size exceeds limits | `PayloadTooLargeDto` |
| 429  | Too Many Requests - Rate limit exceeded | `RateLimitResponse` |
| 500  | Internal Server Error | `ErrorResponseDto` |

### 📊 API Features

- ✅ **File Upload/Download** - Single and bulk operations
- ✅ **Advanced Search** - Full-text search with filtering
- ✅ **Rate Limiting** - Configurable limits per endpoint
- ✅ **Authentication** - JWT-based security
- ✅ **Monitoring** - Health checks and logging
- ✅ **Validation** - Comprehensive input validation

";
        }

        // Add global responses that apply to all operations
        AddGlobalResponses(swaggerDoc);
    }

    private static void AddGlobalResponses(OpenApiDocument swaggerDoc)
    {
        // Define common responses that should be available to reference
        swaggerDoc.Components.Responses ??= new Dictionary<string, OpenApiResponse>();

        if (!swaggerDoc.Components.Responses.ContainsKey("UnauthorizedResponse"))
        {
            swaggerDoc.Components.Responses.Add("UnauthorizedResponse", new OpenApiResponse
            {
                Description = "Unauthorized - Valid JWT token required",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "ErrorResponseDto"
                            }
                        }
                    }
                }
            });
        }

        if (!swaggerDoc.Components.Responses.ContainsKey("ForbiddenResponse"))
        {
            swaggerDoc.Components.Responses.Add("ForbiddenResponse", new OpenApiResponse
            {
                Description = "Forbidden - Insufficient permissions",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "ErrorResponseDto"
                            }
                        }
                    }
                }
            });
        }

        if (!swaggerDoc.Components.Responses.ContainsKey("RateLimitResponse"))
        {
            swaggerDoc.Components.Responses.Add("RateLimitResponse", new OpenApiResponse
            {
                Description = "Too Many Requests - Rate limit exceeded",
                Headers = new Dictionary<string, OpenApiHeader>
                {
                    ["X-RateLimit-Remaining"] = new OpenApiHeader
                    {
                        Description = "Number of requests remaining in the current window",
                        Schema = new OpenApiSchema { Type = "integer" }
                    },
                    ["X-RateLimit-Reset"] = new OpenApiHeader
                    {
                        Description = "UTC date/time when the rate limit window resets",
                        Schema = new OpenApiSchema { Type = "string", Format = "date-time" }
                    }
                },
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "RateLimitResponse"
                            }
                        }
                    }
                }
            });
        }
    }
}

/// <summary>
/// Organizes API operations by logical groupings and priority
/// </summary>
public class TagOrderDocumentFilter : IDocumentFilter
{
    private static readonly Dictionary<string, int> TagOrder = new()
    {
        { "Upload", 1 },
        { "Download", 2 }, 
        { "Search", 3 },
        { "Health", 99 }
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Enhance tag descriptions
        var enhancedTags = new List<OpenApiTag>();

        foreach (var tag in swaggerDoc.Tags ?? new List<OpenApiTag>())
        {
            var enhancedTag = new OpenApiTag
            {
                Name = tag.Name,
                Description = GetEnhancedTagDescription(tag.Name),
                ExternalDocs = GetTagExternalDocs(tag.Name)
            };
            enhancedTags.Add(enhancedTag);
        }

        // Add any missing important tags
        if (!enhancedTags.Any(t => t.Name == "Upload"))
        {
            enhancedTags.Add(new OpenApiTag
            {
                Name = "Upload",
                Description = GetEnhancedTagDescription("Upload")
            });
        }

        if (!enhancedTags.Any(t => t.Name == "Download"))
        {
            enhancedTags.Add(new OpenApiTag
            {
                Name = "Download", 
                Description = GetEnhancedTagDescription("Download")
            });
        }

        if (!enhancedTags.Any(t => t.Name == "Search"))
        {
            enhancedTags.Add(new OpenApiTag
            {
                Name = "Search",
                Description = GetEnhancedTagDescription("Search")
            });
        }

        // Sort tags by priority
        swaggerDoc.Tags = enhancedTags
            .OrderBy(tag => TagOrder.GetValueOrDefault(tag.Name, 50))
            .ThenBy(tag => tag.Name)
            .ToList();

        // Add operation summaries if missing
        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                if (string.IsNullOrEmpty(operation.Summary))
                {
                    operation.Summary = GenerateOperationSummary(operation);
                }
            }
        }
    }

    private static string GetEnhancedTagDescription(string tagName)
    {
        return tagName switch
        {
            "Upload" => "📤 **File Upload Operations**\n\nUpload single files or multiple files in bulk. Supports validation, virus scanning, and metadata attachment.",
            "Download" => "📥 **File Download Operations**\n\nDownload files individually or create bulk archives. Includes resume support and download statistics.",
            "Search" => "🔍 **File Search & Discovery**\n\nPowerful search capabilities with filtering, faceting, and suggestions. Support for full-text search and metadata queries.",
            "Health" => "🏥 **System Health & Monitoring**\n\nHealth check endpoints for monitoring system status and dependencies.",
            _ => $"Operations related to {tagName.ToLowerInvariant()}"
        };
    }

    private static OpenApiExternalDocs? GetTagExternalDocs(string tagName)
    {
        return tagName switch
        {
            "Upload" => new OpenApiExternalDocs
            {
                Description = "Upload API Documentation",
                Url = new Uri("https://docs.yourcompany.com/api/upload")
            },
            "Download" => new OpenApiExternalDocs
            {
                Description = "Download API Documentation", 
                Url = new Uri("https://docs.yourcompany.com/api/download")
            },
            "Search" => new OpenApiExternalDocs
            {
                Description = "Search API Documentation",
                Url = new Uri("https://docs.yourcompany.com/api/search")
            },
            _ => null
        };
    }

    private static string GenerateOperationSummary(OpenApiOperation operation)
    {
        if (operation.Tags?.Any() == true)
        {
            var tag = operation.Tags.First().Name;
            var method = "Operation";
            
            if (operation.OperationId?.Contains("Upload") == true) method = "Upload";
            else if (operation.OperationId?.Contains("Download") == true) method = "Download";
            else if (operation.OperationId?.Contains("Search") == true) method = "Search";
            
            return $"{method} operation for {tag.ToLowerInvariant()}";
        }
        
        return "API Operation";
    }
}