using Next.Abstractions.Trace;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTraceStrategy<TTraceStrategy>(this IServiceCollection services)
            where TTraceStrategy : class, ITraceStrategy
        {
            services.TryAddScoped<ITraceInfoProvider, TraceInfoProvider>();

            return services
                .AddScoped<ITraceStrategy, TTraceStrategy>();
        }
    }
}
