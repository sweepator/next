using System;
using System.Linq;
using Next.Cqrs.Configuration;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusBuilderExtensions
    {
        /// <summary>
        /// Apply a strategy to have a processor for each aggregate root
        /// </summary>
        /// <param name="messageBusBuilder"></param>
        /// <param name="domainMetadataInfo"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IMessageBusBuilder WithProcessorsByAggregateRoot(
            this IMessageBusBuilder messageBusBuilder,
            IDomainMetadataInfo domainMetadataInfo,
            Action<IProcessorBuilder, string> setup = null)
        {
            foreach (var aggregateRootType in domainMetadataInfo.AggregateEventsByAggregateRootType.Keys)
            {
                messageBusBuilder.WithProcessor(o =>
                {
                    var endpoint = aggregateRootType.Name.ToLower();
                    setup?.Invoke(o, endpoint);
                    o.Endpoint(endpoint);

                    var aggregateEventTypes = domainMetadataInfo.AggregateEventsByAggregateRootType[aggregateRootType];
                    foreach (var aggregateEventType in aggregateEventTypes)
                    {
                        o.RegisterDomainEventNotificationHandler(aggregateEventType);
                    }
                    
                    var aggregateCommandsTypes = domainMetadataInfo.AggregateCommandsByAggregateRootType[aggregateRootType];
                    foreach (var aggregateCommandsType in aggregateCommandsTypes)
                    {
                        o.RegisterCommandMessageHandler(aggregateCommandsType);
                    }
                });
            }

            return messageBusBuilder;
        }
        
        /// <summary>
        /// Apply a strategy to have a processor for each aggregate event and command
        /// </summary>
        /// <param name="messageBusBuilder"></param>
        /// <param name="domainMetadataInfo"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IMessageBusBuilder WithProcessorsByAggregateEventAndCommand(
            this IMessageBusBuilder messageBusBuilder,
            IDomainMetadataInfo domainMetadataInfo,
            Action<IProcessorBuilder, AggregateEventEndpoint> setup = null)
        {
            foreach (var aggregateRootType in domainMetadataInfo.AggregateRootTypes)
            {
                foreach (var aggregateEventType in domainMetadataInfo.AggregateEventsByAggregateRootType[aggregateRootType])
                {
                    messageBusBuilder
                        .WithProcessor(o =>
                        {
                            var endpoint = $"{aggregateRootType.Name.ToLower()}.{aggregateEventType.Name.ToLower()}";
                            setup?.Invoke(o, new AggregateEventEndpoint(
                                endpoint,
                                aggregateEventType));
                            
                            o.Endpoint(endpoint)
                                .RegisterDomainEventNotificationHandler(aggregateEventType);
                        });
                }
                
                foreach (var commandType in domainMetadataInfo.AggregateCommandsByAggregateRootType[aggregateRootType])
                {
                    messageBusBuilder
                        .WithProcessor(o =>
                        {
                            var endpoint = $"{aggregateRootType.Name.ToLower()}.{commandType.Name.ToLower()}";
                            o.Endpoint(endpoint)
                                .RegisterCommandMessageHandler(commandType);
                        });
                }
            }

            return messageBusBuilder;
        }
    }
}