using System;
using Next.Abstractions.Bus.Configuration;
using Next.Abstractions.EventSourcing;

namespace Next.Cqrs.Configuration
{
    public interface ICqrsBuilder
    {
        ICqrsBuilder Bus(Action<IMessageBusBuilder> setup);
        ICqrsBuilder EventStore(Action<IEventStoreOptionsBuilder> setup);
        ICqrsBuilder Projections(Action<IProjectionsBuilder> setup);
        ICqrsBuilder Scheduler(Action<ISchedulerBuilder> setup);
        IDomainMetadataInfo DomainMetadataInfo { get; }
    }
}