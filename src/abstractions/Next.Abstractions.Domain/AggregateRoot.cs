using System;
using System.Collections.Generic;

namespace Next.Abstractions.Domain
{
    public abstract class AggregateRoot<TAggregate, TIdentity, TState> : IAggregateRoot<TIdentity, TState>
        where TAggregate : AggregateRoot<TAggregate, TIdentity, TState>
        where TState : IState
        where TIdentity : IIdentity
    {
        private static readonly string AggregateName = typeof(TAggregate).GetAggregateName();
        private readonly List<IDomainEvent> _uncommitedEvents = new List<IDomainEvent>();

        public TState State { get; }

        public virtual string Name => AggregateName;

        public TIdentity Id { get; }

        public int Version { get; }
        
        public virtual bool IsNew => Version <= 0;

        public IEnumerable<IDomainEvent> Changes  => _uncommitedEvents;

        IState IAggregateRoot.State => State;

        protected AggregateRoot(
            TIdentity id, 
            TState state)
        {
            Id = id;
            State = state;
            Version = state.Version;
        }

        protected void On<TEvent>(TEvent aggregateEvent)
            where TEvent : IAggregateEvent<TAggregate>
        {
            if (aggregateEvent == null)
            {
                throw new ArgumentNullException(nameof(aggregateEvent));
            }

            var aggregateSequenceNumber = Version + _uncommitedEvents.Count + 1;
            var eventMetadata = new Metadata
            {
                /*Timestamp = now,
                AggregateSequenceNumber = aggregateSequenceNumber,
                AggregateName = Name.Value,
                AggregateId = Id.Value,
                EventId = eventId*/
            };
            
            var uncommittedEvent = new DomainEvent<TAggregate, TIdentity, TEvent>(
                aggregateEvent,
                eventMetadata,
                DateTime.UtcNow, 
                Id,
                aggregateSequenceNumber);


            State.Mutate(@aggregateEvent);
            _uncommitedEvents.Add(@uncommittedEvent);
        }
    }
}
