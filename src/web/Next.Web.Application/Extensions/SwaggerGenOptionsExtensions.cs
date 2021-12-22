using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Next.Abstractions.Domain;
using Next.Web.Application.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class SwaggerGenOptionsExtensions
    {
        public static SwaggerGenOptions AddCqrsFilters(
            this SwaggerGenOptions swaggerGenOptions,
            Assembly assembly)
        {
            swaggerGenOptions.DocumentFilter<CqrsFilter>();
            swaggerGenOptions.SchemaFilter<CqrsFilter>();
            swaggerGenOptions.ParameterFilter<CqrsFilter>();
            
            var identityTypes = assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Concat(new[] {assembly})
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            typeof(IIdentity).IsAssignableFrom(t))
                .ToList();

            foreach (var identityType in identityTypes)
            {
                swaggerGenOptions.MapType(
                    identityType,
                    () => new OpenApiSchema {Type = "string"});
            }

            return swaggerGenOptions;
        }
    }
}