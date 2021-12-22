using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Data.EntityFramework.Diagnostics
{
    public class QueryTimingObserver : DbCommandInterceptor
    {
        private readonly ILogger<QueryTimingObserver> _logger;

        public QueryTimingObserver(ILogger<QueryTimingObserver> logger)
        {
            _logger = logger;
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Query execution event data: {eventData}", eventData);

            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result)
        {
            _logger.LogDebug("Query execution event data: {eventData}", eventData);
            return base.ReaderExecuted(command, eventData, result);
        }
    }
}
