using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using Next.Abstractions.Serialization.Json;
using Next.Abstractions.Serialization.Xml;
using JsonSerializerDefaults = Next.Abstractions.Serialization.Json.JsonSerializerDefaults;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddXmlSerializer(this IServiceCollection services)
        {
            services.TryAddSingleton<IXmlSerializer, XmlSerializer>();
            return services;
        }
        
        public static IServiceCollection AddJsonSerializer(
            this IServiceCollection services, 
            JsonSerializerOptions options = null)
        {
            services.TryAddSingleton<IJsonSerializer>(new Next.Abstractions.Serialization.Json.JsonSerializer(options));
            return services;
        }
        
        public static IServiceCollection AddJsonSerializer(
            this IServiceCollection services,
            Action<JsonSerializerOptions> setup = null)
        {
            var jsonSerializerOptions = JsonSerializerDefaults.GetDefaultSettings();
            
            setup?.Invoke(jsonSerializerOptions);
            
            services.TryAddSingleton<IJsonSerializer>(new Next.Abstractions.Serialization.Json.JsonSerializer(jsonSerializerOptions));
            return services;
        }
    }
}
