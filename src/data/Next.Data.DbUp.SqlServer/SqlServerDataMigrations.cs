using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DbUp;
using DbUp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Data;

namespace Next.Data.DbUp.SqlServer
{
    public abstract class SqlServerDataMigrations<T> : SqlServerDataMigrations
        where T: SqlServerDataMigrations<T>
    {
        protected SqlServerDataMigrations(
            ILogger<SqlServerDataMigrations<T>> logger,
            IOptionsSnapshot<SqlServerDataMigrationsOptions> options) 
            : base(
                logger, 
                typeof(T).Assembly, 
                options.Get(typeof(T).Assembly.GetName().Name))
        {
        }
    }
    
    public class SqlServerDataMigrations : IDataMigrations
    {
        private const string SqlCommand = "SELECT TOP(1) [ScriptName], [Applied] FROM {0} ORDER BY Applied DESC";
        private readonly ILogger<SqlServerDataMigrations> _logger;
        private readonly Assembly _assembly;
        private readonly SqlServerDataMigrationsOptions _options;

        public SqlServerDataMigrations(
            ILogger<SqlServerDataMigrations> logger,
            Assembly assembly,
            SqlServerDataMigrationsOptions options)
        {
            _logger = logger;
            _assembly = assembly;
            _options = options;
        }

        public async Task MigrateAsync()
        {
            await Task.Run(Migrate);
        }

        public async Task<DataMigration> GetLastMigrationAsync()
        {
            await using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();

            command.CommandText = string.Format(SqlCommand, $"{_options.GetSchema()}.{_options.GetMigrationsTable()}");
            var dataReader = await command.ExecuteReaderAsync();
            if (dataReader.HasRows)
            {
                if (await dataReader.ReadAsync())
                {
                    var lastMigration = dataReader.GetString(0);
                    var timeStamp = dataReader.GetDateTime(1);
                    return new DataMigration(
                        lastMigration,
                        timeStamp);
                }
            }

            return null;
        }

        public void Migrate()
        {
            try
            {
                _logger.Info("Starting data migrations.");
                EnsureDatabase.For.SqlDatabase(_options.ConnectionString);

                var preDeploymentScriptsExecutor =
                    DeployChanges.To
                        .SqlDatabase(_options.ConnectionString)
                        .WithScript(
                            "init", 
                            $@"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{_options.GetSchema()}')
                                    BEGIN
                                    EXEC('CREATE SCHEMA {_options.GetSchema()} AUTHORIZATION dbo');
                                    END")
                        .WithTransactionPerScript()
                        .JournalTo(new NullJournal())
                        .LogToAutodetectedLog()
                        .WithExecutionTimeout(TimeSpan.FromSeconds(_options.TimeoutSeconds))
                        .Build();
                
                var preDeploymentUpgradeResult = preDeploymentScriptsExecutor.PerformUpgrade();
                if (!preDeploymentUpgradeResult.Successful)
                {
                    _logger.LogError("Could not run data migrations successfully: {ErrorScript}", preDeploymentUpgradeResult.ErrorScript.Name);
                    if (preDeploymentUpgradeResult.Error != null)
                    {
                        throw preDeploymentUpgradeResult.Error;
                    }
                }
                
                var builder = DeployChanges.To
                    .SqlDatabase(_options.ConnectionString)
                    .WithScriptsEmbeddedInAssembly(_assembly,
                        s => s.StartsWith($"{_assembly.GetName().Name}.Scripts"))
                    .WithTransactionPerScript()
                    .WithVariables(_options.Variables)
                    .JournalToSqlTable(
                        _options.GetSchema(),
                        _options.GetMigrationsTable())
                    .LogToAutodetectedLog()
                    .WithExecutionTimeout(TimeSpan.FromSeconds(_options.TimeoutSeconds))
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
                throw;
            }
        }
    }
}
