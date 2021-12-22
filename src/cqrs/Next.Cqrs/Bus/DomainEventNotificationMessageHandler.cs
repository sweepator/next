using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Bus
{
    internal class DomainEventNotificationMessageHandler: NotificationMessageHandlerBase<IDomainEvent>
    {
        public DomainEventNotificationMessageHandler(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }
        
        protected override IDomainEvent CreateMessage(
            IServiceScope scope,
            MessageContext messageContext)
        {
            var aggregateEventDefinitionService =
                scope.ServiceProvider.GetRequiredService<IAggregateEventDefinitionService>();
            var domainEventFactory =
                scope.ServiceProvider.GetRequiredService<IDomainEventFactory>();
            
            return messageContext.CreateDomainEvent(
                aggregateEventDefinitionService,
                domainEventFactory);
        }
    }
}