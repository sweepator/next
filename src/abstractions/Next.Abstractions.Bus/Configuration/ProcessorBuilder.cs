using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    /// <summary>
    /// Configurator for message processing capabilities
    /// </summary>
    internal class ProcessorBuilder : IProcessorBuilder
    {
        private static readonly MethodInfo RegisterMessageHandlerMethod =
            typeof(Processor)
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == nameof(RegisterMessageHandler)
                             && m.GetGenericArguments().Length == 1);
        
        private readonly IEnumerable<Type> _allowedMessageTypes;
        internal event Action<Processor> Configuring;
        public event Action<IServiceProvider, IProcessor> OnBuild;
        
        private string _endpoint;
        private int _maxDeliveryCount;
        private bool _deadLetterMessages;
        private int _concurrencyLevel;

        public ProcessorBuilder(IEnumerable<Type> allowedMessageTypes = null)
        {
            _allowedMessageTypes = allowedMessageTypes;
            _endpoint = Assembly.GetExecutingAssembly().GetName().Name;
            _maxDeliveryCount = InboundTransportOptions.DefaultMaxDeliveryCount;
            _concurrencyLevel = InboundTransportOptions.DefaultConcurrencyLevel;
            _deadLetterMessages = InboundTransportOptions.DefaultDeadLetterMessages;
        }

        public IProcessorBuilder Endpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }
        
        public string GetEndpoint()
        {
            return _endpoint;
        }
        
        public IProcessorBuilder MaxDeliveryCount(int maxDeliveryCount)
        {
            _maxDeliveryCount = maxDeliveryCount;
            return this;
        }
        
        public IProcessorBuilder DeadLetterMessages(bool deadLetterMessages)
        {
            _deadLetterMessages = deadLetterMessages;
            return this;
        }
        
        public IProcessorBuilder WithConcurrencyLevel(int concurrencyLevel)
        {
            _concurrencyLevel = concurrencyLevel;
            return this;
        }
        
        public IProcessorBuilder RegisterMessageHandler<TMessage>(
            Func<TMessage, Task> handlerFunc,
            string handlerName = null)
        {
            if (!CheckMessageType(typeof(TMessage)))
            {
                throw new ArgumentException($"Message type not allowed: {typeof(TMessage).Name}");
            }
            
            Configuring += processor => processor.RegisterMessageHandler<TMessage>(
                new MessageHandler<TMessage>(handlerFunc),
                handlerName);

            return this;
        }

        public IProcessorBuilder RegisterMessageHandler(
            Type messageType, 
            IMessageHandler messageHandler,
            string handlerName = null)
        { 
            if (!CheckMessageType(messageType))
            {
                throw new ArgumentException($"Message type not allowed: {messageType.Name}");
            }
            
            Configuring += processor =>
            {
                RegisterMessageHandlerMethod
                    .MakeGenericMethod(messageType)
                    .Invoke(processor, new object []{ messageHandler, handlerName});
            };

            return this;
        }

        /// <summary>
        /// Configures the current processor instance with the previously specified settings.
        /// </summary>
        /// <param name="subscriptionManager">subscriptionManager</param>
        /// <param name="transportFactory">transportFactory</param>
        /// <param name="messageSerializer"></param>
        /// <param name="serviceProvider"></param>
        /// <returns>A configured IProcessor instance</returns>
        public IProcessor Build(
            ISubscriptionManager subscriptionManager,
            ITransportFactory transportFactory,
            IMessageSerializer messageSerializer,
            IServiceProvider serviceProvider)
        {
            var options = new InboundTransportOptions(
                _endpoint, 
                _maxDeliveryCount, 
                _deadLetterMessages);
            
            var processor = new Processor(
                options, 
                _concurrencyLevel,
                messageSerializer,
                subscriptionManager, 
                transportFactory, 
                serviceProvider.GetRequiredService<ILoggerFactory>(),
                _allowedMessageTypes);

            // handlers can be registered here
            Configuring?.Invoke(processor);
            OnBuild?.Invoke(
                serviceProvider, 
                processor);
            
            return processor;
        }

        private bool CheckMessageType(Type messageType)
        {
            if (_allowedMessageTypes == null || !_allowedMessageTypes.Any())
            {
                return true;
            }
            
            return _allowedMessageTypes != null 
                   && _allowedMessageTypes.Contains(messageType);
        }
    }
}