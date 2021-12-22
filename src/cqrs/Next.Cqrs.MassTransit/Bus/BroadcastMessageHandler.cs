using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using MessageContext = Next.Abstractions.Bus.MessageContext;

namespace Next.Cqrs.MassTransit.Bus
{
    internal class BroadcastMessageHandler<TAggregateEvent>: IMessageHandler
        where TAggregateEvent: class, IAggregateEvent
    {
        private readonly IServiceProvider _serviceProvider;

        public BroadcastMessageHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Process(MessageContext messageContext)
        {
            using var scope = _serviceProvider.CreateScope();

            var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var aggregateEventDefinitionService = scope.ServiceProvider.GetRequiredService<IAggregateEventDefinitionService>();
            var domainEventFactory = scope.ServiceProvider.GetRequiredService<IDomainEventFactory>();

            var domainEvent = messageContext.CreateDomainEvent(
                aggregateEventDefinitionService,
                domainEventFactory);
            
            await publisher.Publish(
                (TAggregateEvent)domainEvent.AggregateEvent,
                context =>
                {
                    foreach (var (key, value) in messageContext.Message.Headers)
                    {
                        context.Headers.Set(
                            key, 
                            value);
                    }
                });
        }
    }
}