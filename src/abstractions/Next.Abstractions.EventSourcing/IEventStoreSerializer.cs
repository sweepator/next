using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStoreSerializer
    {
        ISerializedEvent Serialize(IDomainEvent domainEvent);
        IDomainEvent Deserialize(ISerializedEvent @event);
        ISerializedSnapshot Serialize(ISnapshot snapshot);
        IState Deserialize(ISerializedSnapshot @snapshot);
    }
}
