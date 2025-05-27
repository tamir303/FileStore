using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileStoreService.Etc.Models;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);

            for (int i = 0; i < enumNames.Length; i++)
            {
                var enumValue = enumValues.GetValue(i);
                var enumName = enumNames[i];
                
                // Add both numeric value and string name
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiInteger((int)enumValue!));
                
                // Add description if available
                var field = context.Type.GetField(enumName);
                var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                
                if (!string.IsNullOrEmpty(description))
                {
                    if (schema.Description == null)
                        schema.Description = "";
                    schema.Description += $"{enumName}: {description}\n";
                }
            }

            schema.Type = "integer";
            schema.Format = "int32";
        }
    }
}
