using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.EventSourcing.SqlServer;

namespace Next.Abstractions.EventSourcing
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddEventStoreHealthCheck(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            return builder.AddDataMigrationsHealthCheck<SqlServerEventSourcingDataMigrations>(
                string.IsNullOrEmpty(name) ? "evenstore_sql" : name,
                failureStatus,
                tags,
                timeout);
        }
    }
}