using System;
using System.Linq;
using System.Reflection;
using Next.Abstractions.Domain;
using Next.Cqrs.MassTransit.Bus;

namespace Next.Abstractions.Bus.Configuration
{
    public static class ProcessorConfiguratorExtensions
    {
        private static readonly MethodInfo RegisterMessageHandlerMethod =
            typeof(IProcessor)
                .GetTypeInfo()
                .GetMethods()
                .Single(m => m.Name == nameof(IProcessor.RegisterMessageHandler) && m.GetGenericArguments().Length == 1);

        public static IProcessorBuilder BroadcastDomainEvent<TAggregateEvent>(this IProcessorBuilder processorBuilder)
            where TAggregateEvent: class, IAggregateEvent
        {
            return processorBuilder.BroadcastDomainEvent(typeof(TAggregateEvent));
        }
        
        public static IProcessorBuilder BroadcastDomainEvent(
            this IProcessorBuilder processorBuilder,
            Type aggregateEventType)
        {
            processorBuilder.OnBuild += (
                serviceProvider,
                processor) =>
            {
                var messageHandler =
                    (IMessageHandler) Activator.CreateInstance(
                        typeof(BroadcastMessageHandler<>).MakeGenericType(aggregateEventType),
                        serviceProvider);

                RegisterMessageHandlerMethod
                    .MakeGenericMethod(aggregateEventType)
                    .Invoke(processor,
                        new object[]
                        {
                            messageHandler,
                            $"masstransit_broadcast_domain_event_{aggregateEventType.Name.ToLower()}_handler"
                        });
            };
            return processorBuilder;
        }
    }
}
