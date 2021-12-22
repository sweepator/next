using System.Threading.Tasks;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Responsible for processing an incoming message, which involves finding its destination subscription, handler and invoking it.
    /// </summary>
    public interface IMessageDispatcher
    {
        Task<bool> ProcessMessage(TransportMessage message);
    }
}