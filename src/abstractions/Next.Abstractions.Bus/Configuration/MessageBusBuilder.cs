using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public class MessageBusBuilder : IMessageBusBuilder
    {
        private readonly IEnumerable<Type> _allowedMessageTypes;
        private readonly List<ProcessorBuilder> _processorConfigurators;
        private readonly EndpointDiscovery _endpointDiscovery = new();

        public ISubscriptionStore SubscriptionStore { get; private set; }
        
        public ISubscriptionBroker SubscriptionBroker { get; private set; }
        
        public ITransportFactory TransportFactory { get; private set; }
        
        private Func<IServiceProvider, ITransportFactory> TransportFactoryFunc { get; set; }
        
        public IMessageSerializer MessageSerializer { get; private set; }
        
        public IServiceCollection Services { get; }

        public IEnumerable<IProcessorBuilder> ProcessorConfigurators => _processorConfigurators;

        public MessageBusBuilder(
            IServiceCollection services,
            IEnumerable<Type> allowedMessageTypes = null)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            _allowedMessageTypes = allowedMessageTypes;
            _processorConfigurators = new List<ProcessorBuilder>();
        }
        
        public IMessageBusBuilder WithProcessor(Action<IProcessorBuilder> setup)
        {
            Services.TryAddSingleton<IEndpointDiscovery>(_endpointDiscovery);
            
            var processorConfigurator = new ProcessorBuilder(_allowedMessageTypes);
            setup(processorConfigurator);
            
            _endpointDiscovery.RegisterEndpoint(processorConfigurator.GetEndpoint());
            
            Services.AddSingleton(sp =>
            {
                var subscriptionManager = sp.GetRequiredService<ISubscriptionManager>();
                var transportFactory = sp.GetRequiredService<ITransportFactory>();
                var messageSerializer = sp.GetRequiredService<IMessageSerializer>();
                
                return processorConfigurator.Build(
                    subscriptionManager,
                    transportFactory,
                    messageSerializer,
                    sp);
            });
            
            _processorConfigurators.Add(processorConfigurator);

            return this;
        }

        public IMessageBusBuilder WithCustomSubscriptionBroker(ISubscriptionBroker subscriptionBroker)
        {
            SubscriptionBroker = subscriptionBroker;
            Services.TryAddSingleton(SubscriptionBroker);
            Services.TryAddSingleton<ISubscriptionManager, SubscriptionManager>();
            return this;
        }
        
        public IMessageBusBuilder WithCustomSubscriptionStore(ISubscriptionStore subscriptionStore)
        {
            SubscriptionStore = subscriptionStore;
            Services.TryAddSingleton(SubscriptionStore);
            Services.TryAddSingleton<ISubscriptionManager, SubscriptionManager>();
            return this;
        }
        
        public IMessageBusBuilder WithMessageSerializer(IMessageSerializer messageSerializer)
        {
            MessageSerializer = messageSerializer;
            Services.TryAddSingleton(MessageSerializer);
            return this;
        }
        
        public IMessageBusBuilder WithCustomTransport(ITransportFactory transportFactory)
        {
            TransportFactory = transportFactory;
            Services.ReplaceOrAddSingleton(TransportFactory);
            Services.TryAddSingleton(sp =>
                sp.GetRequiredService<ITransportFactory>().CreateOutboundTransport());
            return this;
        }
        
        public IMessageBusBuilder WithCustomTransport(Func<IServiceProvider, ITransportFactory> transportFactoryFunc)
        {
            TransportFactoryFunc = transportFactoryFunc;
            Services.ReplaceOrAddSingleton(TransportFactoryFunc);
            Services.TryAddSingleton(sp => sp.GetRequiredService<ITransportFactory>().CreateOutboundTransport());
            return this;
        }
        
        private ITransportFactory GetTransportFactory(IServiceProvider serviceProvider)
        {
            var transportFactory = TransportFactoryFunc?.Invoke(serviceProvider) ?? TransportFactory;

            if (transportFactory == null)
            {
                throw new ArgumentNullException(nameof(TransportFactory));
            }

            return transportFactory;
        }
        
    }
}