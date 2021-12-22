using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Domain
{
    public interface IAggregateRoot
    {
        IIdentity Id { get; }
        IState State { get; }
        string Name { get; }
        int Version { get; }
        bool IsNew { get; }
        IEnumerable<IDomainEvent> Changes { get; }
        bool HasChanges => Changes.Any();
    }
    
    public interface IAggregateRoot<out TIdentity> : IAggregateRoot
        where TIdentity: IIdentity
    {
        new TIdentity Id { get; }
        
        IIdentity IAggregateRoot.Id => Id;
    }

    public interface IAggregateRoot<out TIdentity, out TState> : IAggregateRoot<TIdentity>
        where TState : IState
        where TIdentity: IIdentity
    {
        new TIdentity Id { get; }
        
        IIdentity IAggregateRoot.Id => Id;
        
        new TState State { get; }
    }
}
