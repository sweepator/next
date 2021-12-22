using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus
{
    public static class MessageBusExtensions
    {
        /// <summary>
        /// Converts any message into a <see cref="Next.Abstractions.Bus.Message"/> type and sends it.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to send</typeparam>
        /// <param name="bus">the bus instance used to send the message</param>
        /// <param name="message">payload to send</param>
        /// <param name="headers">optional headers to be sent</param>
        /// <returns>Task</returns>
        public static Task Send<TMessage>(
            this IMessageBus bus, 
            TMessage message, 
            Dictionary<string, string> headers = null)
        {
            var msg = new Message(message, typeof(TMessage).Name, headers);
            return bus.Send(msg);
        }
    }
}