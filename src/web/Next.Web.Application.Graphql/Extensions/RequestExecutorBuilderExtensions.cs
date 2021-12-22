using System;
using System.Linq;
using System.Net.Mime;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.Cqrs.Commands;
using Next.Cqrs.Configuration;
using Next.Cqrs.Jobs;
using Next.Web.Application.Graphql;

namespace HotChocolate.Execution.Configuration
{
    public static class RequestExecutorBuilderExtensions
    {
        private static readonly DomainEventFactory DomainEventFactory = new();
        
        public static IRequestExecutorBuilder AddCqrs(
            this IRequestExecutorBuilder requestExecutorBuilder,
            IDomainMetadataInfo domainMetadataInfo)
        {
            return requestExecutorBuilder
                .AddWriteModel(domainMetadataInfo)
                .AddReadModel(domainMetadataInfo)
                .AddSubscriptions(domainMetadataInfo);
        }
        
        public static IRequestExecutorBuilder AddReadModel(
            this IRequestExecutorBuilder requestExecutorBuilder,
            IDomainMetadataInfo domainMetadataInfo)
        {
            if(domainMetadataInfo == null)
            {
                throw new ArgumentNullException(nameof(domainMetadataInfo));
            }
            
            requestExecutorBuilder.AddQueryType(new QueryType(domainMetadataInfo));
                
            foreach (var projectionType in domainMetadataInfo.QueryRequestByProjectionTypes.Keys)
            {
                var queryRequestTypes = domainMetadataInfo.QueryRequestByProjectionTypes[projectionType];

                foreach (var queryRequestType in queryRequestTypes)
                {
                    var queryRequestTypeDefinition = typeof(QueryRequestType<,>).MakeGenericType(
                        queryRequestType,
                        projectionType);
                    var projectionTypeDefinition = typeof(ProjectionType<>).MakeGenericType(projectionType);

                    requestExecutorBuilder.AddType(queryRequestTypeDefinition);
                    requestExecutorBuilder.AddType(projectionTypeDefinition);
                }
            }

            return requestExecutorBuilder;
        }
        
        public static IRequestExecutorBuilder AddWriteModel(
            this IRequestExecutorBuilder requestExecutorBuilder,
            IDomainMetadataInfo domainMetadataInfo)
        {
            if(domainMetadataInfo == null)
            {
                throw new ArgumentNullException(nameof(domainMetadataInfo));
            }
            
            requestExecutorBuilder.AddMutationType(new MutationType(domainMetadataInfo));
            requestExecutorBuilder.AddType(new CommandResponseType<CommandResponse>());
                
            foreach (var commandType in domainMetadataInfo.CommandTypes)
            {
                var commandResponseType = commandType.BaseType
                    .GetGenericArguments()
                    .Single(o => typeof(ICommandResponse).IsAssignableFrom(o));
                
                    var commandTypeDefinition = typeof(CommandType<,>).MakeGenericType(
                        commandType,
                        commandResponseType);
                    
                    requestExecutorBuilder.AddType(commandTypeDefinition);

                    if (commandResponseType != typeof(CommandResponse))
                    {
                        var commandResponseTypeDefinition = typeof(CommandResponseType<>).MakeGenericType(commandResponseType);
                        requestExecutorBuilder.AddType(commandResponseTypeDefinition);
                    }
            }

            return requestExecutorBuilder;
        }

        public static IRequestExecutorBuilder AddSubscriptions(
            this IRequestExecutorBuilder requestExecutorBuilder,
            IDomainMetadataInfo domainMetadataInfo)
        {
            if (domainMetadataInfo == null)
            {
                throw new ArgumentNullException(nameof(domainMetadataInfo));
            }

            requestExecutorBuilder.AddSubscriptionType(new SubscriptionType(domainMetadataInfo));

            foreach (var aggregateEventType in domainMetadataInfo.AggregateEventTypes)
            {
                var domainEventTypeDefinition = typeof(AggregateEventType<>).MakeGenericType(aggregateEventType);
                requestExecutorBuilder.AddType(domainEventTypeDefinition);
                
                var domainEventType = DomainEventFactory.GetDomainEventType(aggregateEventType);
                var notificationType = typeof(Notification<>).MakeGenericType(domainEventType);

                var handlerType = typeof(TopicDomainEventSenderMessageHandler<,>)
                    .MakeGenericType(
                        notificationType,
                        aggregateEventType);
                var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
                requestExecutorBuilder
                    .Services
                    .AddTransient(handlerInterfaceType, handlerType);
            }

            return requestExecutorBuilder;
        }
    }
}