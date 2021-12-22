using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Next.Web.OpenApi.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    
    /*
     *
     * 
     */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenerationConfiguration(
            this IServiceCollection services,
            Assembly assembly,
            string applicationName,
            Action<SwaggerGenOptions> setup = null)
        {
            services
                .AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp => 
                    new ConfigureSwaggerOptions(
                        sp.GetRequiredService<IApiVersionDescriptionProvider>(), 
                        applicationName))
                .AddSwaggerGen(o =>
                {
                    var xmlDocs = assembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { assembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(assembly.Location), $"{a.Name}.xml"))
                        .Where(File.Exists)
                        .ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        o.IncludeXmlComments(d);
                    });
                    
                    setup?.Invoke(o);
                });

            return services;
        }
    }
}