using System;
using System.Linq;
using System.Threading.Tasks;
using Next.Abstractions.Data;
using Microsoft.Extensions.Logging;
using System.Reflection;
using DbUp;

namespace Next.Data.DbUp.PostgreSql
{
    public class PostgreSqlDataMigrations : IDataMigrations
    {
        private readonly ILogger<PostgreSqlDataMigrations> _logger;
        private readonly Assembly _assembly;
        private readonly string _connectionString;
        private readonly double _timeoutSeconds;

        public PostgreSqlDataMigrations(
            ILogger<PostgreSqlDataMigrations> logger,
            Assembly assembly,
            string connectionString,
            double timeoutSeconds)
        {
            _logger = logger;
            _assembly = assembly;
            _connectionString = connectionString;
            _timeoutSeconds = timeoutSeconds;
        }

        public async Task MigrateAsync()
        {
            await Task.Run(Migrate);
        }

        public Task<DataMigration> GetLastMigrationAsync()
        {
            throw new NotImplementedException();
        }

        public void Migrate()
        {
            try
            {
                EnsureDatabase.For.PostgresqlDatabase(_connectionString);

                var builder =  DeployChanges.To
                    .PostgresqlDatabase(_connectionString)
                    .WithScriptsEmbeddedInAssembly(_assembly,
                        s => s.StartsWith($"{_assembly.GetName().Name}.Scripts"))
                    .WithTransactionPerScript()
                    .LogToAutodetectedLog()
                    .WithExecutionTimeout(TimeSpan.FromSeconds(_timeoutSeconds))
                    .Build();

                var result = builder.PerformUpgrade();

                if (!result.Successful)
                {
                    _logger.LogError("Could not run data migrations successfully: {ErrorScript}", result.ErrorScript.Name);
                    if (result.Error != null)
                    {
                        throw result.Error;
                    }
                }
                else
                {
                    var scripts = string.Join(',', result.Scripts);
                    _logger.Info("Finished data migrations: {ScriptsCount} {Scripts}", result.Scripts.Count(), scripts);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on executing data migrations.");
            }
        }
    }
}
