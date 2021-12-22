using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.Abstractions.Data;
using Next.Data.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddDataMigrationsHealthCheck<TDataMigrations>(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
            where TDataMigrations : class, IDataMigrations
        {
            return builder.AddCheck<DataMigrationsHealthCheck<TDataMigrations>>(
                string.IsNullOrEmpty(name) ? "datamigrations" : name,
                failureStatus,
                tags);
        }
    }
}