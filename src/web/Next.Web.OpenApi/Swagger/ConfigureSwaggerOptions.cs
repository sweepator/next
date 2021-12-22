using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Next.Web.OpenApi.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
        private readonly string _applicationName;

        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            string applicationName)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            _applicationName = applicationName;
        }
        
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, _applicationName));
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(
            ApiVersionDescription description,
            string applicationName)
        {
            var info = new OpenApiInfo()
            {
                Title = applicationName,
                Version = description.ApiVersion.ToString()
            };
            
            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}