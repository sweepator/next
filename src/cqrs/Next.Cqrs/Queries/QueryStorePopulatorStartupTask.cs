using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Health;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public class QueryStorePopulatorStartupTask<TProjectionModel> : StartupTask
        where TProjectionModel: IProjectionModel
    {
        private readonly IQueryStorePopulator _queryStorePopulator;
        private readonly IOptions<QueryStorePopulatorStartupOptions<TProjectionModel>> _options;
        
        public override string Name => $"QueryStorePopulatorStartupTask:{typeof(TProjectionModel).Name}";

        public QueryStorePopulatorStartupTask(
            ILogger<QueryStorePopulatorStartupTask<TProjectionModel>> logger,
            StartupTaskContext startupTaskContext,
            IQueryStorePopulator queryStorePopulator,
            IOptions<QueryStorePopulatorStartupOptions<TProjectionModel>> options)
            : base(logger, startupTaskContext)
        {
            _queryStorePopulator = queryStorePopulator;
            _options = options;
        }

        protected override async Task Work(CancellationToken cancellationToken = default)
        {
            if (!_options.Value.Enabled)
            {
                return;
            }
            
            var projectionModelType = typeof(TProjectionModel);

            if (_options.Value.Rebuild)
            {
                await _queryStorePopulator.Purge(projectionModelType);
            }

            await _queryStorePopulator.Populate(projectionModelType);
        }
    }
}