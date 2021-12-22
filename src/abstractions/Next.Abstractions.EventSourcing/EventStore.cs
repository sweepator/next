using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Data;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing.Metadata;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public sealed class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IEventStoreSerializer _eventStoreSerializer;
        private readonly IEventStoreBus _eventStoreBus;
        private readonly IEnumerable<IMetadataEnricher> _metadataEnrichers;
        private readonly IEnumerable<ISnapshotStrategy> _snapshotStrategies;
        private readonly IOutboxStoreListener _outboxStoreListener;
        private readonly Lazy<IEnumerable<IEventStoreSyncProcessor>> _eventStoreSyncProcessors;
        private readonly IOptions<EventPublisherOptions> _options;
        private readonly ILogger<IEventStore> _logger;

        public EventStore(
            IEventStoreRepository eventStoreRepository,
            IEventStoreSerializer eventStoreSerializer,
            IEventStoreBus eventStoreBus,
            IEnumerable<IMetadataEnricher> metadataEnrichers,
            ISnapshotRepository snapshotRepository,
            IEnumerable<ISnapshotStrategy> snapshotStrategies, 
            IOutboxStoreListener outboxStoreListener,
            Lazy<IEnumerable<IEventStoreSyncProcessor>> eventStoreSyncProcessors,
            IOptions<EventPublisherOptions> options,
            ILogger<EventStore> logger)
        {
            _eventStoreRepository = eventStoreRepository;
            _snapshotRepository = snapshotRepository;
            _eventStoreSerializer = eventStoreSerializer;
            _eventStoreBus = eventStoreBus;
            _metadataEnrichers = metadataEnrichers;
            _snapshotStrategies = snapshotStrategies;
            _outboxStoreListener = outboxStoreListener;
            _eventStoreSyncProcessors = eventStoreSyncProcessors;
            _options = options;
            _logger = logger;
        }
        
        public async Task<IEnumerable<IDomainEvent>> Load<TAggregateRoot, TIdentity>(TIdentity id) 
            where TAggregateRoot : IAggregateRoot<TIdentity> 
            where TIdentity : IIdentity
        {
            await CheckAndProcessUncommittedEvents(id);
            
            var serializedEvents = await _eventStoreRepository.Get(id);
           
            var domainEvents = serializedEvents
                .Select(e => _eventStoreSerializer.Deserialize(e))
                .ToList();
            
            return domainEvents;
        }
        
        public async Task Append<TAggregateRoot, TIdentity>(    
            TIdentity id,
            int expectedVersion, 
            IEnumerable<IDomainEvent> events) 
            where TAggregateRoot : IAggregateRoot<TIdentity> 
            where TIdentity : IIdentity
        {
            var transaction = DomainTransaction.Current;
            var transactionId = transaction?.Id ?? Guid.NewGuid();
            
            
            await CheckAndProcessUncommittedEvents(id);
            
            var serializedEvents = events
                .Select(domainEvent =>
                {
                    // update transaction id metadata value
                    domainEvent.Metadata.TransactionId = transactionId;
                    
                    // enrich domain event metadata with all available enrichers
                    _metadataEnrichers
                        .ToList()
                        .ForEach(o=>o.Enrich(domainEvent));
 
                    return _eventStoreSerializer.Serialize(domainEvent);
                })
                .ToList();
            
            await _eventStoreRepository.Save(
                id,
                transactionId,
                expectedVersion,
                serializedEvents,
                async () => await ProcessSyncProcessors(events));

            await ProcessAndCommitEvents(typeof(TAggregateRoot),
                id,
                transactionId,
                events);
        }

        public async Task<Snapshot<TAggregateRoot, TIdentity, TState>> GetLastSnapshot<TAggregateRoot, TState, TIdentity>(TIdentity id) 
            where TAggregateRoot : IAggregateRoot<TIdentity, TState> 
            where TState : class, IState 
            where TIdentity : IIdentity
        {
            var serializedSnapshot = await _snapshotRepository.GetSnapshot(id);

            if (serializedSnapshot == null)
            {
                return null;
            }

            _logger.LogDebug(
                "Snapshot found for {AggregateType} with id {Identity} and version {Version}",
                serializedSnapshot.AggregateName,
                serializedSnapshot.AggregateId,
                serializedSnapshot.Version);
            
            var state = (TState)_eventStoreSerializer.Deserialize(serializedSnapshot);
            return new Snapshot<TAggregateRoot, TIdentity, TState>
            (
                id,
                state,
                serializedSnapshot.Timestamp
            );
        }

        public async Task AddSnapshot(ISnapshot snapshot)
        {
            _logger.LogDebug(
                "Adding snapshot for {AggregateType} with id {Identity} and version {Version}",
                snapshot.AggregateType.Name,
                snapshot.AggregateIdentity.Value,
                snapshot.AggregateVersion);
            
            var serializedSnapshot = _eventStoreSerializer.Serialize(snapshot);
            await _snapshotRepository.SaveSnapshot(serializedSnapshot);
            
            _logger.LogDebug(
                "Snapshot added for {AggregateType} with id {Identity} and version {Version}",
                snapshot.AggregateType.Name,
                snapshot.AggregateIdentity.Value,
                snapshot.AggregateVersion);
        }

        public async Task<IEnumerable<IDomainEvent>> LoadRange<TAggregateRoot, TIdentity>(
            TIdentity id, 
            int start, 
            int? end = null) 
            where TAggregateRoot : IAggregateRoot<TIdentity> 
            where TIdentity : IIdentity
        {
            var serializedEvents = await _eventStoreRepository.GetRange(
                id,
                start,
                end);

            var domainEvents = serializedEvents
                .Select(e => _eventStoreSerializer.Deserialize(e))
                .ToList();
            
            return domainEvents;
        }
        
        public async Task<IPagedList<IDomainEvent>> LoadAllEvents(PageSelection pageSelection)
        {
            if (pageSelection.Size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSelection.Size));
            }

            var allCommittedEventsPage = await _eventStoreRepository
                .GetCommitted(pageSelection)
                .ConfigureAwait(false);
            
            return allCommittedEventsPage.ConvertTo(se => se
                .Select(e => _eventStoreSerializer.Deserialize(e))
                .ToList());
        }
        
        private async Task CheckAndProcessUncommittedEvents(IIdentity identity)
        {
            if (!_options.Value.InlineProcessorEnabled)
            {
                return;;
            }
            
            var uncommittedEvents = await _eventStoreRepository.GetUncommitted(identity);
            var domainEventsByTransaction = uncommittedEvents
                .Select(e => _eventStoreSerializer.Deserialize(e))
                .GroupBy(o => o.Metadata.TransactionId)
                .ToDictionary(o => o.Key, o => o.ToList());

            foreach (var transactionId in domainEventsByTransaction.Keys)
            {
                var domainEvents = domainEventsByTransaction[transactionId];
                var aggregateType = domainEvents.Select(o => o.AggregateType).First();

                _logger.LogDebug(
                    "Processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                    domainEvents.Count,
                    aggregateType.Name,
                    identity.Value,
                    transactionId);
                
                try
                {
                    await _eventStoreBus.Publish(domainEvents);
                    await _eventStoreRepository.Commit(
                        identity,
                        transactionId);
                    
                    _logger.LogDebug(
                        "Processed {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                        domainEvents.Count,
                        aggregateType.Name,
                        identity.Value,
                        transactionId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Error on processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                        domainEvents.Count,
                        aggregateType.Name,
                        identity.Value,
                        transactionId);
                    break;
                }
            }
        }

        private async Task ProcessAndCommitEvents(
            Type aggregateRootType,
            IIdentity id,
            Guid transactionId,
            IEnumerable<IDomainEvent> events)
        {
            if (_options.Value.BackgroundProcessorEnabled)
            {
                _outboxStoreListener.NotifyEventToProcess(events);
            }
            
            if (!_options.Value.InlineProcessorEnabled)
            {
                return;
            }

            try
            {
                _logger.LogDebug(
                    "Processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                    events.Count(),
                    aggregateRootType.Name,
                    id.Value,
                    transactionId);

                await _eventStoreBus.Publish(events);
                await _eventStoreRepository.Commit(
                    id,
                    transactionId);

                _logger.LogDebug(
                    "Processed {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                    events.Count(),
                    aggregateRootType.Name,
                    id.Value,
                    transactionId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e,
                    "Error on processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                    events.Count(),
                    aggregateRootType.Name,
                    id.Value,
                    transactionId);
            }
        }
        
        private async Task ProcessSyncProcessors(IEnumerable<IDomainEvent> events)
        {
            foreach (var eventSyncProcessor in _eventStoreSyncProcessors.Value)
            {
                await eventSyncProcessor.Process(events);
            }
        }
    }
}