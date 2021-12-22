using Next.Web.Trace;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpTraceStrategy(this IServiceCollection services)
        {
            return services
                .AddTraceStrategy<HttpTraceStrategy>();
        }
    }
}
