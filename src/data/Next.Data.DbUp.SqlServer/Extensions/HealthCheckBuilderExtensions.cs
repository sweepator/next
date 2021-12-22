using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.Abstractions.Data;
using Next.Data.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddDataMigrationsHealthCheck<T>(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        where T: IDataMigrations
        {
            return builder.AddCheck<DataMigrationsHealthCheck<T>>(
                string.IsNullOrEmpty(name) ? "datamigrations" : name,
                failureStatus,
                tags,
                timeout);
        }
    }
}