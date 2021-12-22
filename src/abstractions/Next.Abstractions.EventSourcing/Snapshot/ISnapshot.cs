using System;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISnapshot
    {
        Type AggregateType { get; }

        IIdentity AggregateIdentity { get; }
        
        int AggregateVersion { get; }
        
        IState State { get; }
        
        DateTime Timestamp { get; }
    }
    
    public interface ISnapshot<out TAggregate, out TIdentity, out TState> : ISnapshot
        where TAggregate : IAggregateRoot<TIdentity, TState>
        where TIdentity : IIdentity
        where TState : IState
    {
        new TState State { get; }
        
        IState ISnapshot.State => State;
        
        new TIdentity AggregateIdentity { get; }
        
        IIdentity ISnapshot.AggregateIdentity => AggregateIdentity;
    }
}