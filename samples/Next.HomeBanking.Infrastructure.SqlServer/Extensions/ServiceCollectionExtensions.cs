using System;
using Next.HomeBanking.Infrastructure.SqlServer;
using Next.Data.DbUp.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHomeBankingDataMigrations(
            this IServiceCollection services,
            Action<SqlServerDataMigrationsOptions> setup)
        {
            return services.AddSqlServerDataMigrations<SqlServerHomeBankingDataMigrations>(setup);
        }
    }
}
