using System;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Default implementation for a message handler that delegates the handling to a Func it receives
    /// </summary>
    /// <typeparam name="TMessage">Type of the message to handle</typeparam>
    public class MessageHandler<TMessage> : IMessageHandler
    {
        private readonly Func<TMessage, Task> _handlerFunc;

        public MessageHandler(Func<TMessage, Task> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        public Task Process(MessageContext messageContext)
        {
            var message = messageContext.GetMessage<TMessage>();
            return _handlerFunc(message);
        }
    }
}