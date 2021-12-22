using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Domain;
using Next.Cqrs.Commands;
using Next.Cqrs.Jobs;

namespace Next.Cqrs.Configuration
{
    public interface ISchedulerBuilder
    {
        IServiceCollection Services { get; }
        
        IDomainEventFactory DomainEventFactory { get; }

        ISchedulerBuilder CommandFor<TCommand, TCommandResponse, TAggregateEvent>(Action<CommandSchedulerOptions<TCommand, TCommandResponse>> setup = null)
            where TCommand : class, ICommand<TCommandResponse>
            where TCommandResponse : ICommandResponse
            where TAggregateEvent : IAggregateEvent;
    }
}