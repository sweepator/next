using System;
using System.Linq;
using System.Reflection;
using Next.Abstractions.Domain;
using Next.Cqrs.Bus;
using Next.Cqrs.Commands;

namespace Next.Abstractions.Bus.Configuration
{
    public static class ProcessorConfiguratorExtensions
    {
        private static readonly MethodInfo RegisterForCommandMethod =
            typeof(IProcessor)
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m =>
                    m.Name == nameof(IProcessor.RegisterMessageHandler)
                    && m.GetParameters().Length == 2);
        
        
        /// <summary>
        /// Register a subscriber for a certain aggregate event
        /// </summary>
        /// <param name="processorBuilder"></param>
        /// <typeparam name="TAggregateEvent"></typeparam>
        /// <returns></returns>
        public static IProcessorBuilder RegisterDomainEventNotificationHandler<TAggregateEvent>(this IProcessorBuilder processorBuilder)
            where TAggregateEvent: IAggregateEvent
        {
            processorBuilder.OnBuild += (
                serviceProvider, 
                processor) =>
            {
                processor.RegisterMessageHandler<TAggregateEvent>(
                    new DomainEventNotificationMessageHandler(serviceProvider),
                    "domain_events_handler");
            };
            return processorBuilder;
        }

        /// <summary>
        /// Register a subscriber for a certain aggregate event
        /// </summary>
        /// <param name="processorBuilder"></param>
        /// <param name="aggregateEventType"></param>
        /// <returns></returns>
        public static IProcessorBuilder RegisterDomainEventNotificationHandler(
            this IProcessorBuilder processorBuilder,
            Type aggregateEventType)
        {
            if (!typeof(IAggregateEvent).GetTypeInfo().IsAssignableFrom(aggregateEventType))
            {
                throw new ApplicationException("Aggregate event type expected.");
            }
            
            processorBuilder.OnBuild += (
                serviceProvider, 
                processor) =>
            {
                processor.RegisterMessageHandler(
                    aggregateEventType,
                    new DomainEventNotificationMessageHandler(serviceProvider),
                    "domain_events_handler");
            };
            return processorBuilder;
        }

        /// <summary>
        /// Register command handler for a certain command
        /// </summary>
        /// <param name="processorBuilder"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="TCommandResponse"></typeparam>
        /// <returns></returns>
        public static IProcessorBuilder RegisterCommandMessageHandler<TCommand, TCommandResponse>(this IProcessorBuilder processorBuilder)
            where TCommand: ICommand<TCommandResponse>
            where TCommandResponse: ICommandResponse
        {
            processorBuilder.OnBuild += (
                serviceProvider, 
                processor) =>
            {
                processor.RegisterMessageHandler<TCommand>(
                    new CommandMessageHandler<TCommand, TCommandResponse>(serviceProvider),
                    "command_handler");
            };
            return processorBuilder;
        }
        
        /// <summary>
        /// Register command handler for a certain command
        /// </summary>
        /// <param name="processorBuilder"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static IProcessorBuilder RegisterCommandMessageHandler(
            this IProcessorBuilder processorBuilder,
            Type commandType)
        {
            if (!typeof(ICommand).GetTypeInfo().IsAssignableFrom(commandType))
            {
                throw new ApplicationException("Command type expected.");
            }
            
            processorBuilder.OnBuild += (
                serviceProvider, 
                processor) =>
            {
                var commandResponseType = commandType.BaseType?
                    .GetGenericArguments()
                    .FirstOrDefault(o => typeof(ICommandResponse).GetTypeInfo().IsAssignableFrom(o));

                if (commandResponseType == null)
                {
                    throw new ApplicationException("Command response type expected.");
                }

                var commandHandlerType = typeof(CommandMessageHandler<,>).MakeGenericType(
                    commandType,
                    commandResponseType);

                var handler = Activator.CreateInstance(
                    commandHandlerType,
                    serviceProvider
                );

                RegisterForCommandMethod
                    .MakeGenericMethod(commandType)
                    .Invoke(
                        processor,
                        new[]
                        {
                            handler,
                            "command_handler"
                        });
            };
            return processorBuilder;
        }
    }
}
