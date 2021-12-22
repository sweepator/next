using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Domain;
using Next.Cqrs.Commands;
using Next.Cqrs.Jobs;

namespace Next.Cqrs.Configuration
{
    internal class SchedulerBuilder : ISchedulerBuilder
    {
        public IDomainEventFactory DomainEventFactory { get; }
        public IServiceCollection Services { get; }
        
        public SchedulerBuilder(
            IDomainEventFactory domainEventFactory,
            IServiceCollection services)
        {
            DomainEventFactory = domainEventFactory;
            Services = services;
        }
        
        public ISchedulerBuilder CommandFor<TCommand, TCommandResponse, TAggregateEvent>(Action<CommandSchedulerOptions<TCommand, TCommandResponse>> setup = null)
            where TCommand: class, ICommand<TCommandResponse>
            where TCommandResponse : ICommandResponse
            where TAggregateEvent: IAggregateEvent
        {
            var domainEventType = DomainEventFactory.GetDomainEventType(typeof(TAggregateEvent));
            var notificationType = typeof(Application.Contracts.Notification<>).MakeGenericType(domainEventType);

            var jobHandlerType = typeof(CommandSchedulerNotificationHandler<,,>).MakeGenericType(
                notificationType, 
                typeof(TCommand),
                typeof(TCommandResponse));
            
            var jobHandlerInterfaceType = typeof(MediatR.INotificationHandler<>).MakeGenericType(notificationType);
            
            // register job handler
            Services
                .AddTransient(jobHandlerInterfaceType, jobHandlerType);
            Services
                .AddTransient<CommandJob<TCommand, TCommandResponse>>();

            Services
                .Configure(setup);
            
            return this;
        }
    }
}