using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus.Configuration;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;

namespace Next.Cqrs.Configuration
{
    internal class CqrsBuilder: ICqrsBuilder
    {
        private Action<IEventStoreOptionsBuilder> _eventStoreSetup;
        private Action<IProjectionsBuilder> _projectionsSetup;
        private Action<ISchedulerBuilder> _schedulerSetup;
        
        private MessageBusBuilder MessageBusBuilder { get; }

        public IDomainEventFactory DomainEventFactory { get; }
        
        public IDomainMetadataInfo DomainMetadataInfo { get; }

        internal CqrsBuilder(
            IServiceCollection services,
            IDomainEventFactory domainEventFactory,
            IDomainMetadataInfo domainMetadataInfo)
        {
            DomainEventFactory = domainEventFactory;
            DomainMetadataInfo = domainMetadataInfo;
            
            MessageBusBuilder = new MessageBusBuilder(
                services,
                DomainMetadataInfo.AggregateEventTypes);

            // set in memory as default
            MessageBusBuilder.WithInMemory();
        }

        /// <summary>
        /// Configures messaging capabilities
        /// </summary>
        /// <param name="setup">setup handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ICqrsBuilder Bus(Action<IMessageBusBuilder> setup)
        {
            setup(MessageBusBuilder);
            return this;
        }

        public ICqrsBuilder EventStore(Action<IEventStoreOptionsBuilder> setup)
        {
            _eventStoreSetup = setup;
            return this;
        }

        public ICqrsBuilder Projections(Action<IProjectionsBuilder> setup)
        {
            _projectionsSetup = setup;
            return this;
        }
        
        public ICqrsBuilder Scheduler(Action<ISchedulerBuilder> setup)
        {
            _schedulerSetup = setup;
            return this;
        }
        
        internal void Build(IServiceCollection services)
        {
            services.AddMessageBus();

            // setup event store service registration if specified 
            if (_eventStoreSetup != null)
            {
                services.AddEventStore(
                    DomainMetadataInfo.SnapshotStateTypes,
                    _eventStoreSetup);
            }
            
            // projections logic service registration
            var projectionsBuilder = new ProjectionsBuilder(
                services,
                DomainMetadataInfo.QueryRequestByProjectionTypes);
            _projectionsSetup?.Invoke(projectionsBuilder);
            
            // scheduler logic
            var schedulerBuilder = new SchedulerBuilder(
                DomainEventFactory,
                services);
            _schedulerSetup?.Invoke(schedulerBuilder);
        }
    }
}