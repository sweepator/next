using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Processes incoming messages based on the subscriptions registered for the current process.
    /// Allows a process to subscribe and/or unsubscribe to specific messages
    /// </summary>
    public interface IProcessor : IDisposable
    {
        /// <summary>
        /// Indicates whether the current processor is running and processing incoming messages
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        ///  Gets the current processor inbound transport options
        /// </summary>
        InboundTransportOptions InboundTransportOptions { get; }
        
        IEnumerable<Type> AllowedMessageTypes { get; }
        
        /// <summary>
        /// Starts processing incoming messages
        /// </summary>
        Task Start();

        /// <summary>
        /// Stops processing incoming messages
        /// </summary>
        void Stop();

        public void RegisterMessageHandler(
            Type messageType,
            IMessageHandler handler,
            string handlerName = null);

        public void RegisterMessageHandler<TMessage>(
            IMessageHandler handler,
            string handlerName = null);
        
        public void RegisterMessageHandlerForAll(
            IMessageHandler handler,
            string handlerName = null);
    }
}