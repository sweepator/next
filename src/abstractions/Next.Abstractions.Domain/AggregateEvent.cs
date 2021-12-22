namespace Next.Abstractions.Domain
{
    public abstract class AggregateEvent<TAggregate> : IAggregateEvent<TAggregate>
        where TAggregate : IAggregateRoot
    {
        public virtual string GetEventName()
        {
            return GetType().Name;
        }
    }
}
