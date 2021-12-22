using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Data;
using Next.Abstractions.Health;

namespace Next.Data.Health
{
    public class DataMigrationsStartupTask<TDataMigrations> : StartupTask
        where TDataMigrations: IDataMigrations
    {
        public override string Name => typeof(TDataMigrations).Name;

        public IDataMigrations DataMigrations { get; }

        public DataMigrationsStartupTask(
            ILogger<DataMigrationsStartupTask<TDataMigrations>> logger,
            StartupTaskContext startupTaskContext,
            TDataMigrations dataMigrations)
            : base(logger, startupTaskContext)
        {
            DataMigrations = dataMigrations;
        }


        protected override async Task Work(CancellationToken cancellationToken = default)
        {
            await DataMigrations.MigrateAsync();
        }
    }
}
