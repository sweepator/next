using MassTransit;
using Next.Abstractions.Bus.Transport;
using Next.Bus.MassTransit.Configuration;
using Next.Bus.MassTransit.Transport;

namespace Next.Bus.Masstransit.Transport
{
    internal class MassTransitTransportFactory : BrokerlessTransportFactory
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ISendEndpointStrategy _sendEndpointStrategy;
        private readonly MassTransitBusConfiguration _configuration;
        
        public MassTransitTransportFactory(
            ISendEndpointProvider sendEndpointProvider,
            ISendEndpointStrategy sendEndpointStrategy,
            MassTransitBusConfiguration configuration)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _sendEndpointStrategy = sendEndpointStrategy;
            _configuration = configuration;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new MassTransitTransport(
                _sendEndpointProvider,
                _sendEndpointStrategy,
                _configuration,
                endpoint);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new MassTransitTransport(
                _sendEndpointProvider,
                _sendEndpointStrategy,
                _configuration);
        }
    }
}