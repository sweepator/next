using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Data.DbUp.SqlServer;

namespace Next.EventSourcing.SqlServer
{
    internal sealed class SqlEventSourcingDataMigrations: SqlDataMigrations
    {
        public SqlEventSourcingDataMigrations(
            ILogger<SqlDataMigrations> logger,
            IOptionsMonitor<SqlDataMigrationsOptions> options)
            : base(
                 logger,
                 typeof(SqlEventSourcingDataMigrations).Assembly,
                 options.Get(nameof(SqlEventSourcingDataMigrations)))
        {
        }
    }
}
