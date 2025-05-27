using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileStoreService.Etc.Models;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                       p.ParameterType == typeof(List<IFormFile>) ||
                       p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (!fileUploadParams.Any())
            return;

        // Clear existing parameters for file upload endpoints
        operation.Parameters?.Clear();

        // Set request body for file upload
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        // Add file parameter(s)
        foreach (var param in fileUploadParams)
        {
            if (param.ParameterType == typeof(IFormFile))
            {
                schema.Properties[param.Name ?? "file"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else if (param.ParameterType == typeof(IEnumerable<IFormFile>) ||
                     param.ParameterType == typeof(List<IFormFile>) ||
                     param.ParameterType == typeof(IFormFile[]))
            {
                schema.Properties[param.Name ?? "files"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
        }

        // Add other form parameters
        var otherParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType != typeof(IFormFile) && 
                       p.ParameterType != typeof(IEnumerable<IFormFile>) &&
                       p.ParameterType != typeof(List<IFormFile>) &&
                       p.ParameterType != typeof(IFormFile[]) &&
                       p.GetCustomAttribute<FromBodyAttribute>() == null)
            .ToList();

        foreach (var param in otherParams)
        {
            var paramName = param.Name ?? "parameter";
            var paramType = GetOpenApiType(param.ParameterType);
            
            schema.Properties[paramName] = new OpenApiSchema
            {
                Type = paramType.Type,
                Format = paramType.Format
            };
        }
    }

    private (string Type, string? Format) GetOpenApiType(Type type)
    {
        if (type == typeof(string))
            return ("string", null);
        if (type == typeof(int) || type == typeof(int?))
            return ("integer", "int32");
        if (type == typeof(long) || type == typeof(long?))
            return ("integer", "int64");
        if (type == typeof(bool) || type == typeof(bool?))
            return ("boolean", null);
        if (type == typeof(DateTime) || type == typeof(DateTime?))
            return ("string", "date-time");
        if (type == typeof(Guid) || type == typeof(Guid?))
            return ("string", "uuid");

        return ("string", null);
    }
}