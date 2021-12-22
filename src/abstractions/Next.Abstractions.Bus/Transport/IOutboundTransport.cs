using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus.Transport
{
    /// <summary>
    /// Represents a message transport where messages can be sent to
    /// </summary>
    public interface IOutboundTransport : ITransport
    {
        Task Send(TransportMessage message);
        Task SendMultiple(IEnumerable<TransportMessage> messages);
    }
}