using System;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestStories.API.Infrastructure.Filters
{
    /// <inheritdoc />
    public class NullableTypeSchemaFilter : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model.Properties == null)
            {
                return;
            }

            foreach (var (key, value) in model.Properties)
            {
                var property = context.Type.GetProperty(key,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null || !property.PropertyType.IsGenericType ||
                    property.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)) continue;
                value.Default = null;
                //value.Extensions.Add("nullable", true);
                value.Example = null;
            }
        }
    }
}
