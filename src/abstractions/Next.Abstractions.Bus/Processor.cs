using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Diagnostics;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    public class Processor : 
        IProcessor,
        IMessageDispatcher
    {
        private readonly InboundTransportOptions _options;
        private readonly IMessageSerializer _messageSerializer;
        private readonly MessageWorker _worker;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly HashSet<Subscription> _subscriptions;
        private readonly ILogger<Processor> _logger;
        private readonly Dictionary<string, Dictionary<string, IMessageHandler>> _registry;

        public InboundTransportOptions InboundTransportOptions => _options;
        
        public IEnumerable<Type> AllowedMessageTypes { get; }

        public Processor(
            InboundTransportOptions options,
            int concurrencyLevel,
            IMessageSerializer messageSerializer,
            ISubscriptionManager subscriptionManager,
            ITransportFactory transportFactory,
            ILoggerFactory loggerFactory,
            IEnumerable<Type> allowedMessageTypes = null)
        {
            _options = options;
            _messageSerializer = messageSerializer;
            _subscriptionManager = subscriptionManager;
            AllowedMessageTypes = allowedMessageTypes;

            _subscriptions = new HashSet<Subscription>();
            _registry = new Dictionary<string, Dictionary<string, IMessageHandler>>();
            _logger = loggerFactory.CreateLogger<Processor>();

            _worker = new MessageWorker(
                transportFactory.CreateInboundTransport(options),
                new DiagnosticsDispatcher(
                    this,
                    loggerFactory.CreateLogger<DiagnosticsDispatcher>()),
                loggerFactory,
                concurrencyLevel);
        }

        public void RegisterMessageHandler<TMessage>(
            IMessageHandler handler,
            string handlerName = null)
        {
            RegisterMessageHandler(
                typeof(TMessage),
                handler,
                handlerName);
        }

        public void RegisterMessageHandlerForAll(
            IMessageHandler handler, 
            string handlerName = null)
        {
            if (AllowedMessageTypes != null)
            {
                foreach (var allowedMessageType in AllowedMessageTypes)
                {
                    RegisterMessageHandler(
                        allowedMessageType,
                        handler,
                        handlerName);
                }
            }
        }

        public void RegisterMessageHandler(
            Type messageType,
            IMessageHandler handler, 
            string handlerName = null)
        {
            var subscription = new Subscription(
                messageType.Name, 
                _options.Endpoint, 
                handlerName ?? Guid.NewGuid().ToString());

            if (_subscriptions.Contains(subscription))
            {
                throw new InvalidOperationException($"Duplicate subscription for {subscription.Id}");
            }

            CreateSubscription(
                subscription, 
                handler);
        }

        private void CreateSubscription(
            Subscription subscription, 
            IMessageHandler handler)
        {
            // add subscription to notify subscription manager
            _subscriptions.Add(subscription);

            // register handler to process incoming messages on this subscription
            if (!_registry.TryGetValue(subscription.Topic, out var handlers))
            {
                handlers = _registry[subscription.Topic] = new Dictionary<string, IMessageHandler>();
            }

            handlers[subscription.Component] = handler;
        }

        private IMessageHandler GetMessageHandler(
            string messageName, 
            string handlerName)
        {
            // try to get handler by message name (topic)
            return GetMessageHandlerByName(
                messageName,
                handlerName);
        }

        private IMessageHandler GetMessageHandlerByName(
            string messageName, 
            string handlerName)
        {
            if (_registry.TryGetValue(messageName, out var handlers))
            {
                if (handlers.TryGetValue(handlerName, out var handler))
                {
                    return handler;
                }
            }
            
            return null;
        }

        async Task<bool> IMessageDispatcher.ProcessMessage(TransportMessage message)
        {
            var handlerName = message.Headers[MessageHeaders.Component];
            var key = $"{message.Name}:{message.Headers[MessageHeaders.Component]}";
            _logger.LogDebug("Processing {Key} in endpoint {Endpoint}",
                key,
                _options.Endpoint);
            
            // find handler
            var handler = GetMessageHandler(message.Name, handlerName);
            
            if (handler == null)
            {
                _logger.LogWarning("Handler {HandlerName} not found or doesn't process message {MessageName} in processor with endpoint {Endpoint}",
                    handlerName,
                    message.Name,
                    _options.Endpoint);
                return false;
            }

            // create MessageContext and process
            var context = new MessageContext(
                _messageSerializer, 
                message);
            
            await handler.Process(context);
            _logger.LogDebug("{Key} processed in endpoint {Endpoint}",
                key,
                _options.Endpoint);

            return true;
        }

        public async Task Start()
        {
            // notify subscription manager about the subscriptions for this processor
            await _subscriptionManager.UpdateEndpointSubscriptions(
                _options.Endpoint,
                _subscriptions);

            // start processing incoming messages
            _worker.Start();
        }

        public void Stop()
        {
            // stops processing incoming messages
            _worker.Stop();
        }

        public bool IsRunning => _worker.IsRunning;

        public void Dispose()
        {
            _worker.Dispose();
        }
    }
}