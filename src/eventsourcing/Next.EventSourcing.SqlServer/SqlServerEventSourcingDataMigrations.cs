using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Data.DbUp.SqlServer;

namespace Next.EventSourcing.SqlServer
{
    internal sealed class SqlServerEventSourcingDataMigrations: SqlServerDataMigrations<SqlServerEventSourcingDataMigrations>
    {
        public SqlServerEventSourcingDataMigrations(
            ILogger<SqlServerEventSourcingDataMigrations> logger,
            IOptionsSnapshot<SqlServerDataMigrationsOptions> options)
            : base(logger, options)
        {
        }
    }
}