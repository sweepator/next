using System;
using Next.Data.DbUp.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerDataMigrations<TDataMigrations>(
            this IServiceCollection services,
            Action<SqlServerDataMigrationsOptions> setup)
            where TDataMigrations : SqlServerDataMigrations
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            var name = typeof(TDataMigrations).Assembly.GetName().Name;
            services
                .AddOptions<SqlServerDataMigrationsOptions>(name)
                .Configure(setup);

            services.AddDataMigrationsStartupTask<TDataMigrations>();
            return services;
        }
    }
}

    