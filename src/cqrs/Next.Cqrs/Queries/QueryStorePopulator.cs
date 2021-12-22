using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Data;
using Next.Abstractions.EventSourcing;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public class QueryStorePopulator : IQueryStorePopulator
    {
        private readonly ILogger<QueryStorePopulator> _logger;
        private readonly IOptions<QueryStorePopulatorOptions> _options;
        private readonly IEventStore _eventStore;
        private readonly IEnumerable<IQueryStore> _queryStores;
        private readonly IEnumerable<ISyncQueryStoreUpdater> _syncQueryStores;
        private readonly IEnumerable<IAsyncQueryStoreUpdater> _asyncQueryStoreUpdaters;
        private static readonly Type[] ProjectionModelTypes = new[]
        {
            typeof( IProjectionModelFor<,,> )
        };

        public QueryStorePopulator(
            ILogger<QueryStorePopulator> logger,
            IOptions<QueryStorePopulatorOptions> options,
            IEventStore eventStore,
            IEnumerable<ISyncQueryStoreUpdater> syncQueryStores,
            IEnumerable<IAsyncQueryStoreUpdater> asyncQueryStoreUpdaters, 
            IEnumerable<IQueryStore> queryStores)
        {
            _logger = logger;
            _options = options;
            _eventStore = eventStore;
            _syncQueryStores = syncQueryStores;
            _asyncQueryStoreUpdaters = asyncQueryStoreUpdaters;
            _queryStores = queryStores;
        }
        
        public async Task Purge<TProjectionModel>() 
            where TProjectionModel : class, IProjectionModel
        {
            await Purge(typeof(TProjectionModel));
        }

        public async Task Populate<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel
        {
            await Populate(typeof(TProjectionModel));
        }

        public async Task Populate(Type projectionModelType)
        {
            var stopwatch = Stopwatch.StartNew();
            var asyncQueryStoreUpdaters = GetAsyncQueryStoreUpdaters(projectionModelType);
            var syncQueryStoreUpdaters = GetSyncQueryStoreUpdaters(projectionModelType);

            var aggregateEventTypes = new HashSet<Type>(projectionModelType
                .GetTypeInfo()
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType 
                            && ProjectionModelTypes.Contains(i.GetGenericTypeDefinition()))
                .Select(i => i.GetTypeInfo().GetGenericArguments()[2]));
            
            _logger.LogInformation(
                "Projection model {ProjectionModelType} is interested in these aggregate events: {AggregateEventTypes}",
                projectionModelType.Name,
                aggregateEventTypes.Select(e => e.Name));
            

            long totalEvents = 0;
            long relevantEvents = 0;
            var currentPosition = new PageSelection
            {
                Number = 1,
                Size = _options.Value.ChunkSize
            };

            while (true)
            {
                _logger.LogDebug(
                    "Loading events starting from {CurrentPosition} and the next {PageSize} for populating {ProjectionModelType}",
                    currentPosition,
                    currentPosition.Size,
                    projectionModelType.Name);
                
                var allEventsPage = await _eventStore.LoadAllEvents(currentPosition)
                    .ConfigureAwait(false);
                
                totalEvents += allEventsPage.Count();
                currentPosition.Number++;

                if (!allEventsPage.Any())
                {
                    _logger.LogDebug(
                        "No more events in event store, stopping population of projection model {ProjectionModelType}",
                        projectionModelType.Name);
                    break;
                }

                var domainEvents = allEventsPage
                    .Where(e => aggregateEventTypes.Contains(e.EventType))
                    .ToList();
                relevantEvents += domainEvents.Count;

                if (!domainEvents.Any())
                {
                    continue;
                }

                var applyTasks = asyncQueryStoreUpdaters
                    .Select(m => m.Update(domainEvents))
                    .ToList();
                applyTasks.AddRange(syncQueryStoreUpdaters
                    .Select(m => m.Update(domainEvents)));
                
                await Task.WhenAll(applyTasks).ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Population of projection model {ProjectionModelType} took {Milliseconds} ms, in which {TotalEventCount} events was loaded and {RelevantEventCount} was relevant",
                projectionModelType.Name,
                stopwatch.Elapsed.TotalMilliseconds,
                totalEvents,
                relevantEvents);
        }

        public async Task Purge(Type projectionModelType)
        {
            var queryStores = GetQueryStores(projectionModelType);

            var deleteTasks = queryStores.Select(s => s.DeleteAll());
            await Task
                .WhenAll(deleteTasks)
                .ConfigureAwait(false);
        }
        
        public async Task Delete(
            object id, 
            Type projectionModelType)
        {
            var queryStores = GetQueryStores(projectionModelType);

            _logger.LogDebug(
                "Deleting projection model {ProjectionModelType} with ID {Id}",
                projectionModelType.Name,
                id);

            var deleteTasks = queryStores.Select(s => s.Delete(id));
            await Task.WhenAll(deleteTasks).ConfigureAwait(false);
        }
        
        private IEnumerable<ISyncQueryStoreUpdater> GetSyncQueryStoreUpdaters(Type projectionModelType)
        {
            return _syncQueryStores
                .Where(o => o.ProjectionModelType == projectionModelType)
                .ToList();
        }
        
        private IEnumerable<IAsyncQueryStoreUpdater> GetAsyncQueryStoreUpdaters(Type projectionModelType)
        {
            return _asyncQueryStoreUpdaters
                .Where(o => o.ProjectionModelType == projectionModelType)
                .ToList();
        }
        
        private IEnumerable<IQueryStore> GetQueryStores(Type projectionModelType)
        {
            return _queryStores
                .Where(o => o.ProjectionModelType == projectionModelType)
                .ToList();
        }
    }
}