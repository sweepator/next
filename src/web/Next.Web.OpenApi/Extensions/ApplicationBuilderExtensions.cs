using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDefaults(
            this IApplicationBuilder applicationBuilder,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            string applicationName = null,
            string routePrefix = null,
            Action<SwaggerOptions> setupSwagger = null,
            Action<SwaggerUIOptions> setupSwaggerUi = null)
        {
            applicationBuilder
                .UseSwagger(setupSwagger)
                .UseSwaggerUI(o =>
                {
                    setupSwaggerUi?.Invoke(o);

                    if (!string.IsNullOrWhiteSpace(applicationName))
                    {
                        o.DocumentTitle = applicationName;
                    }
                    
                    // build a swagger endpoint for each discovered API version
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        o.SwaggerEndpoint(
                            string.IsNullOrWhiteSpace(routePrefix)
                                ? $"/swagger/{description.GroupName}/swagger.json"
                                : $"/{routePrefix}/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });

            return applicationBuilder;
        }
    }
}