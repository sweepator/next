using Next.Data.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbInitializerStartupTask(this IServiceCollection services)
        {
            return services
                .AddStartupTask<MongoDbInitializerStartupTask>();
        }
    }
}
