using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Web.Application.Graphql
{ 
    public class TopicDomainEventSenderMessageHandler<TNotification, TAggregateEvent> : NotificationHandler<TNotification>
        where TNotification : INotification<IDomainEvent>
        where TAggregateEvent : IAggregateEvent
    {
        private readonly ILogger<TopicDomainEventSenderMessageHandler<TNotification, TAggregateEvent>> _logger;
        private readonly ITopicEventSender _topicEventSender;

        public TopicDomainEventSenderMessageHandler(
            ILogger<TopicDomainEventSenderMessageHandler<TNotification, TAggregateEvent>> logger,
            ITopicEventSender topicEventSender)
        {
            _logger = logger;
            _topicEventSender = topicEventSender;
        }
        
        public override async Task Execute(
            TNotification notification,
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            var topic = notification.Content.AggregateEvent.GetEventName().ToLower();
            var aggregateEvent = (TAggregateEvent)notification.Content.AggregateEvent;
            
            _logger.LogDebug("Sending event {AggregateEvent} to topic {Topic} for graphql subscribers",
                aggregateEvent,
                topic);
            
            await _topicEventSender.SendAsync(
                topic,
                aggregateEvent, 
                cancellationToken);
            
            _logger.LogDebug("Event {AggregateEvent} sent to topic {Topic} for graphql subscribers",
                aggregateEvent,
                topic);
        }
    }
}