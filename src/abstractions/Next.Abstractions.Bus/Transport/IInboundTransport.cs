using System;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus.Transport
{
    /// <summary>
    /// Represents a message transport where messages can be received from
    /// </summary>
    public interface IInboundTransport : ITransport
    {
        Task<IMessageTransaction> Receive(TimeSpan timeout);
    }
}