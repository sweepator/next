using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public abstract class QueryStoreUpdater<TQueryStore, TProjectionModel> : 
        IAsyncQueryStoreUpdater<TProjectionModel>,
        ISyncQueryStoreUpdater<TProjectionModel>
        where TQueryStore : IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        private readonly ILogger _logger;
        private readonly TQueryStore _queryStore;
        
        private static readonly Type ProjectionModelType = typeof(TProjectionModel);
        private static readonly ISet<Type> AggregateEventTypes;
        
        static QueryStoreUpdater()
        {
            var iAmReadModelForInterfaceTypes = ProjectionModelType
                .GetTypeInfo()
                .GetInterfaces()
                .Where(IsProjectionModelFor)
                .ToList();
                
            if (!iAmReadModelForInterfaceTypes.Any())
            {
                throw new ArgumentException($"Projection type '{ProjectionModelType.Name}' does not implement any '{typeof(IProjectionModelFor<,,>).Name}'");
            }

            AggregateEventTypes = new HashSet<Type>(iAmReadModelForInterfaceTypes.Select(i => i.GetTypeInfo().GetGenericArguments()[2]));
            if (AggregateEventTypes.Count != iAmReadModelForInterfaceTypes.Count)
            {
                throw new ArgumentException($"Read model type '{ProjectionModelType.Name}' implements ambiguous '{typeof(IProjectionModelFor<,,>).Name}' interface");
            }
        }
        
        protected QueryStoreUpdater(
            ILogger logger,
            TQueryStore queryStore)
        {
            _logger = logger;
            _queryStore = queryStore;
        }
        
        private static bool IsProjectionModelFor(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                return false;
            }
            
            var typeDefinition = type.GetGenericTypeDefinition();
            return typeDefinition == typeof(IProjectionModelFor<,,>);
        }
        
        public async Task Update(
            IEnumerable<IDomainEvent> domainEvents, 
            CancellationToken cancellationToken = default) 
        {
            var relevantDomainEvents = domainEvents
                .Where(e => AggregateEventTypes.Contains(e.EventType))
                .ToList();
            
            if (!relevantDomainEvents.Any())
            {
                _logger.LogDebug($"None of these events was relevant for read model {typeof(TProjectionModel).Name}, skipping update: {relevantDomainEvents.Select(e => e.ToString()).ToList()}");
                return;
            }
            
            var updates = BuildProjectionModelUpdates(relevantDomainEvents);
            
            if (!updates.Any())
            { 
                _logger.LogDebug($"No projection model updates after building for read model {typeof(TProjectionModel).Name} in store {typeof(TQueryStore).Name} with these events: {relevantDomainEvents.Select(e => e.ToString()).ToList()}");
                return;
            }

            await _queryStore.Update(
                    updates,
                    new ProjectionModelContextFactory(),
                    Update,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        
        protected abstract IEnumerable<ProjectionModelUpdate> BuildProjectionModelUpdates(IEnumerable<IDomainEvent> domainEvents);
        
        protected abstract Task<ProjectionModelUpdateResult<TProjectionModel>> Update(
            IProjectionModelContext projectionModelContext,
            IEnumerable<IDomainEvent> domainEvents,
            ProjectionModelEnvelope<TProjectionModel> projectionModelEnvelope,
            CancellationToken cancellationToken = default);
    }
}