using Next.Application.Log;
using System;
using System.Linq;
using System.Text.Json;
using Next.Abstractions.Serialization.Json;
using JsonSerializer = Next.Abstractions.Serialization.Json.JsonSerializer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogStep(
            this IServiceCollection services,
            Action<ILogOptionsBuilder> setup = null)
        {
            if (!services.Any(x => x.ServiceType == typeof(IJsonSerializer)))
            {
                throw new ApplicationException(
                    "Must register IJsonSerializer in service collection to be used in logstep default configuration");
            }
            
            setup?.Invoke(new LogOptionsBuilder(services));
            return services.AddLogStep<LogSerializer>();
        }
        
        public static IServiceCollection AddLogStep<TLogSerializer>(
            this IServiceCollection services,
            Action<ILogOptionsBuilder> setup = null)
            where TLogSerializer: class, ILogStepSerializer
        {
            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(LogStep<,>));
            services.AddScoped<ILogStepSerializer, TLogSerializer>();
            setup?.Invoke(new LogOptionsBuilder(services));
            return services;
        }
        
        public static IServiceCollection AddLogStep(
            this IServiceCollection services,
            Action<JsonSerializerOptions> jsonSetup,
            Action<ILogOptionsBuilder> setup = null)
        {
            if (jsonSetup == null)
            {
                throw new ArgumentNullException(nameof(jsonSetup));
            }

            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSetup(jsonSerializerOptions);

            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(LogStep<,>));
            services.AddScoped<ILogStepSerializer>(sp => new LogSerializer(new JsonSerializer(jsonSerializerOptions)));
            setup?.Invoke(new LogOptionsBuilder(services));
            return services;
        }
    }
}
