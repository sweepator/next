using System;
using Next.Abstractions.Domain;

namespace Next.Abstractions.Bus
{
    public static class MessageContextExtensions
    {
        public static IDomainEvent CreateDomainEvent(
            this MessageContext messageContext,
            IAggregateEventDefinitionService aggregateEventDefinitionService,
            IDomainEventFactory domainEventFactory)
        {
            var metadata = new Metadata();
            metadata.AddRange(messageContext.Message.Headers);

            var aggregateId = messageContext.Message.Headers[Cqrs.Bus.MessageHeaders.AggregateId];
            var version = int.Parse(messageContext.Message.Headers[Cqrs.Bus.MessageHeaders.EventVersion]);
            var eventVersion = int.Parse(messageContext.Message.Headers[Cqrs.Bus.MessageHeaders.EventSchemaVersion]);
            var timestamp = DateTime.FromBinary(long.Parse(messageContext.Message.Headers[Cqrs.Bus.MessageHeaders.DateUtc]));

            var eventDefinition = aggregateEventDefinitionService.GetDefinition(
                messageContext.Message.Name,
                eventVersion);

            var aggregateEvent = (IAggregateEvent) messageContext.MessageSerializer.Deserialize(
                eventDefinition.Type,
                messageContext.Message.PayLoad);

            var domainEvent = domainEventFactory.Create(
                aggregateEvent,
                metadata,
                aggregateId,
                version,
                timestamp);

            return domainEvent;
        }
    }
}