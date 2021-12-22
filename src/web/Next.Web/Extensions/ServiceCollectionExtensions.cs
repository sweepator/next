using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Next.Abstractions.Serialization.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add IJsonSerializer service configured with json options
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultJsonSerializer(this IServiceCollection services)
        {
            services.TryAddSingleton<IJsonSerializer>(sp =>
            {
                var jsonSerializerOptions = sp.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions;
                return new JsonSerializer(jsonSerializerOptions);
            });

            return services;
        }
    }
}