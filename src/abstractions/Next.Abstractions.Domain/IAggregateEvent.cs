namespace Next.Abstractions.Domain
{
    public interface IAggregateEvent
    {
        string GetEventName();
    }

    public interface IAggregateEvent<out TAggregate> : IAggregateEvent
        where TAggregate : IAggregateRoot
    {
    }
}
