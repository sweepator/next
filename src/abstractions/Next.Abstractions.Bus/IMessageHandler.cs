using System.Threading.Tasks;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Represents a message handler that can handle a specific message.
    /// This is used internally. Each message subscription has an associated IMessageHandler instance.
    /// </summary>
    public interface IMessageHandler
    {
        Task Process(MessageContext messageContext);
    }
}