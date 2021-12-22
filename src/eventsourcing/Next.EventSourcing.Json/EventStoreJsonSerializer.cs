using System;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.Abstractions.Serialization.Json;

namespace Next.EventSourcing.Json
{
    public class EventStoreJsonSerializer : IEventStoreSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IAggregateEventDefinitionService _aggregateEventDefinitionService;
        private readonly ISnapshotDefinitionService _snapshotDefinitionService;
        private readonly IDomainEventFactory _domainEventFactory;

        public EventStoreJsonSerializer(
            IJsonSerializer jsonSerializer,
            IAggregateEventDefinitionService aggregateEventDefinitionService,
            IDomainEventFactory domainEventFactory, 
            ISnapshotDefinitionService snapshotDefinitionService)
        {
            _jsonSerializer = jsonSerializer;
            _aggregateEventDefinitionService = aggregateEventDefinitionService;
            _domainEventFactory = domainEventFactory;
            _snapshotDefinitionService = snapshotDefinitionService;
        }
        
        public ISerializedEvent Serialize(IDomainEvent domainEvent)
        {
            var aggregateEvent = domainEvent.AggregateEvent;
            var eventDefinition = _aggregateEventDefinitionService.GetDefinition(aggregateEvent.GetType());

            var metadata = new Metadata
            {
                SchemaVersion = eventDefinition.Version
            };

            if (domainEvent.Metadata != null)
            {
                foreach (var key in domainEvent.Metadata.Keys)
                {
                    if (!metadata.ContainsKey(key))
                    {
                        metadata.Add(key, domainEvent.Metadata[key]);
                    }
                }
            }
            
            var dataJson = _jsonSerializer.Serialize(aggregateEvent);
            var metaJson = _jsonSerializer.Serialize(metadata);

            return new SerializedEvent(
                domainEvent.AggregateIdentity.Value,
                domainEvent.AggregateType.Name,
                domainEvent.AggregateEvent.GetEventName(),
                domainEvent.Version,
                domainEvent.Timestamp,
                dataJson,
                metaJson);
        }

        public IDomainEvent Deserialize(ISerializedEvent @event)
        {
            var metadata = (IMetadata)_jsonSerializer.Deserialize<Metadata>(@event.Metadata);
            
            // set internal metadata values
            metadata.TransactionId = @event.TransactionId.GetValueOrDefault();
            metadata.Commited = @event.Commited;

            return Deserialize(
                metadata,
                @event.AggregateId,
                @event.Data,
                @event.EventName,
                @event.Version,
                @event.Timestamp);
        }

        public ISerializedSnapshot Serialize(ISnapshot snapshot)
        {
            var state = snapshot.State;
            var snapshotVersionDefinition = _snapshotDefinitionService.GetDefinition(state.GetType());

            var metadata = new Metadata
            {
                SchemaVersion = snapshotVersionDefinition.Version
            };

            var dataJson = _jsonSerializer.Serialize(state);
            var metaJson = _jsonSerializer.Serialize(metadata);

            return new SerializedSnapshot(
                snapshot.AggregateIdentity.Value,
                snapshot.AggregateType.Name,
                state.GetType().Name,
                state.Version,
                snapshot.Timestamp,
                dataJson,
                metaJson);
        }

        public IState Deserialize(ISerializedSnapshot snapshot)
        {
            var metadata = (IMetadata)_jsonSerializer.Deserialize<Metadata>(@snapshot.Metadata);
            
            var eventDefinition = _snapshotDefinitionService.GetDefinition(
                snapshot.Name,
                metadata.SchemaVersion);
            
            var state = (IState)_jsonSerializer.Deserialize(
                eventDefinition.Type,
                snapshot.Data);

            return state;
        }

        private IDomainEvent Deserialize(
            IMetadata metadata,
            string aggregateId,
            string json,
            string eventName,
            int version,
            DateTime timestamp)
        {
            var eventDefinition = _aggregateEventDefinitionService.GetDefinition(
                eventName,
                metadata.SchemaVersion);

            var aggregateEvent = (IAggregateEvent)_jsonSerializer.Deserialize(
                eventDefinition.Type,
                json);
            
            var domainEvent = _domainEventFactory.Create(
                aggregateEvent,
                metadata,
                aggregateId,
                version,
                timestamp);
            
            return domainEvent;
        }
    }
}