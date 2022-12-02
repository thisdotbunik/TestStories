using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestStories.API.Infrastructure.Filters
{
    /// <inheritdoc />
    public class DefaultValueSchemaFilter : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null)
            {
                return;
            }

            foreach (var propertyInfo in context.Type.GetProperties())
            {

                var defaultAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultAttribute != null)
                {
                    foreach (var (key, value) in schema.Properties)
                    {
                        if (ToCamelCase(propertyInfo.Name) == key)
                        {
                            value.Example = (Microsoft.OpenApi.Any.IOpenApiAny)defaultAttribute.Value;
                            break;
                        }
                    }
                }
            }
        }

        private static string ToCamelCase(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
