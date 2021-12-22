using System;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public sealed class Snapshot<TAggregate, TIdentity, TState> : ISnapshot<TAggregate, TIdentity, TState>
        where TAggregate : IAggregateRoot<TIdentity, TState>
        where TIdentity : IIdentity
        where TState: IState
    {
        public Type AggregateType => typeof(TAggregate);
        public TIdentity AggregateIdentity { get; }
        public int AggregateVersion { get; }
        public TState State { get; }
        public DateTime Timestamp { get; }
        
        public Snapshot(
            TIdentity identity,
            TState state,
            DateTime timestamp)
        {
            State = state;
            Timestamp = timestamp;
            AggregateVersion = state.Version;
            AggregateIdentity = identity;
        }
    }
}