using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Next.Abstractions.Bus;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;

namespace Next.Cqrs.Bus
{
    public class DomainEventBus: IDomainEventBus, IEventStoreBus
    {
        private readonly IMessageBus _messageBus;
        private readonly IAggregateEventDefinitionService _aggregateEventDefinitionService;

        public DomainEventBus(
            IMessageBus messageBus,
            IAggregateEventDefinitionService aggregateEventDefinitionService)
        {
            _messageBus = messageBus;
            _aggregateEventDefinitionService = aggregateEventDefinitionService;
        }
        
        public async Task Publish(IDomainEvent @event)
        {
            var messages = BuildMessage(@event);
            await _messageBus.Send(messages);
        }

        public async Task Publish(IEnumerable<IDomainEvent> events)
        {
            var messages = events.Select(BuildMessage).ToArray();
            await _messageBus.Send(messages);
        }
        
        private Message BuildMessage(IDomainEvent ev)
        {
            var eventDefinition = _aggregateEventDefinitionService.GetDefinition(ev.EventType);
            var transactionId = ev.Metadata.TransactionId;
            
            var headers = new Dictionary<string, string>
            {
                { MessageHeaders.DateUtc, ev.Timestamp.ToBinary().ToString() },
                { MessageHeaders.AggregateId, ev.AggregateIdentity.Value },
                { MessageHeaders.AggregateType, ev.AggregateType.Name },
                { MessageHeaders.AggregateEvent, ev.AggregateEvent.GetEventName() },
                { MessageHeaders.EventVersion, ev.Version.ToString() },
                { MessageHeaders.EventSchemaVersion, eventDefinition.Version.ToString() },
                { MessageHeaders.TransactionId, transactionId.ToString() }
            };

            var message = new Message(
                ev.AggregateEvent, 
                ev.AggregateEvent.GetEventName(), 
                headers);
            
            return message;
        }
    }
}