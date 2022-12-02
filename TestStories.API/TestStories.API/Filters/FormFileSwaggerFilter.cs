using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestStories.API.Infrastructure.Filters
{
    /// <inheritdoc />
    public class FormFileSwaggerFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.HttpMethod == HttpMethods.Post)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            var isFormFileFound = false;

            foreach (var parameter in operation.Parameters)
            {
                if (!(parameter is OpenApiParameter nonBodyParameter)) continue;
                var methodParameter =
                    context.ApiDescription.ParameterDescriptions.FirstOrDefault(x => x.Name == parameter.Name);
                if (methodParameter != null)
                {
                    if (typeof(IFormFile).IsAssignableFrom(methodParameter.Type))
                    {
                        nonBodyParameter.Name= "file";
                        nonBodyParameter.In = ParameterLocation.Header;
                        isFormFileFound = true;
                    }
                    else if (typeof(IEnumerable<IFormFile>).IsAssignableFrom(methodParameter.Type))
                    {
                        nonBodyParameter.Name = "file";
                        nonBodyParameter.In = ParameterLocation.Header;
                        isFormFileFound = true;
                    }
                }

                parameter.Name = char.ToLowerInvariant(parameter.Name[0]) + parameter.Name.Substring(1);
            }

            var formFileParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(x => x.ParameterType == typeof(IFormFile)).ToList();
            foreach (var apiParameterDescription in formFileParameters)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = apiParameterDescription.Name,
                    In = ParameterLocation.Header,
                    Description = "Upload File",
                    Required = true
                });
            }

            if (formFileParameters.Any())
            {
                foreach (var propertyInfo in typeof(IFormFile).GetProperties())
                {
                    var parametersWithTheSameName = operation.Parameters.Where(x => x.Name == propertyInfo.Name);
                    foreach (var parameterWithTheSameName in parametersWithTheSameName)
                    {
                        operation.Parameters.Remove(parameterWithTheSameName);
                    }
                }
            }
            
            if (isFormFileFound)
            {
                //operation.Consumes.Add("multipart/form-data");
            }
        }
    }
}
