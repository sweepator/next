using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Data.DbUp.SqlServer;

namespace Next.HomeBanking.Infrastructure.SqlServer
{
    internal sealed class SqlServerHomeBankingDataMigrations: SqlServerDataMigrations<SqlServerHomeBankingDataMigrations>
    {
        public SqlServerHomeBankingDataMigrations(
            ILogger<SqlServerHomeBankingDataMigrations> logger,
            IOptionsSnapshot<SqlServerDataMigrationsOptions> options)
            : base(logger, options)
        {
        }
    }
}