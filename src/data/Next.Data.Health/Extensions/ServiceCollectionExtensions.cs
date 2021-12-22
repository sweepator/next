using Next.Abstractions.Data;
using Next.Data.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataMigrationsStartupTask<TDataMigrations>(this IServiceCollection services)
            where TDataMigrations : class, IDataMigrations
        {
            return services
                .AddStartupTask<DataMigrationsStartupTask<TDataMigrations>>()
                .AddSingleton<TDataMigrations>();
        }
    }
}
