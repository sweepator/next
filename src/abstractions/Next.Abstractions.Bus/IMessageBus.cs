using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Represents a message bus abstraction that can be used to send one or multiple messages.
    /// </summary>
    public interface IMessageBus
    {
        Task Send(Message message);
        Task Send(IEnumerable<Message> messages);

        Task Start();
        Task Stop();
    }
}