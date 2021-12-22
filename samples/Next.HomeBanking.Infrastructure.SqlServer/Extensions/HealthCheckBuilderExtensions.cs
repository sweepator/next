using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.HomeBanking.Infrastructure.SqlServer;
using Next.Data.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddCatalogDataMigrationsHealthCheck(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            return builder.AddCheck<DataMigrationsHealthCheck<SqlServerHomeBankingDataMigrations>>(
                string.IsNullOrEmpty(name) ? "catalog_sql" : name,
                failureStatus,
                tags,
                timeout);
        }
    }
}