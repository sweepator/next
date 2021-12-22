using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.Abstractions.Data;

namespace Next.Data.Health
{
    public class DataMigrationsHealthCheck<TDataMigrations>  : IHealthCheck
        where TDataMigrations: IDataMigrations
    {
        private readonly TDataMigrations _dataMigrations;

        public DataMigrationsHealthCheck(TDataMigrations dataMigrations)
        {
            _dataMigrations = dataMigrations;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var lastMigration = await _dataMigrations.GetLastMigrationAsync();
                var data = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()
                {
                    { "lastMigration", lastMigration.MigrationName },
                    { "timeStamp", lastMigration.Timestamp.ToUniversalTime().ToString("O")}
                });

                return HealthCheckResult.Healthy(data: data);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus, 
                    exception: ex);
            }
        }
    }
}