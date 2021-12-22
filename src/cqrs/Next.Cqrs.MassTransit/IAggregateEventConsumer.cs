using MassTransit;
using Next.Abstractions.Domain;

namespace Next.Cqrs.MassTransit
{
    public interface IAggregateEventConsumer<in TAggregateEvent> : IConsumer<TAggregateEvent>
        where TAggregateEvent : class, IAggregateEvent
    {
    }
}