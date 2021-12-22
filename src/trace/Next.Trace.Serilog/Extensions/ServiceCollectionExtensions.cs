using Next.Trace.Serilog.Enrichers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add trace enricher for serilog
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTraceEnricher(this IServiceCollection services)
        {
            services.TryAddScoped<ILogEventEnricher, TraceEnricher>();
            return services;
        }
    }
}
