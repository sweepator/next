namespace Next.Abstractions.Bus.Transport
{
    public abstract class BrokerlessTransportFactory : ITransportFactory
    {
        IInboundTransport ITransportFactory.CreateInboundTransport(InboundTransportOptions options)
        {
            var inbound = CreateInboundTransport(options.Endpoint);
            var outbound = CreateOutboundTransport();

            return new RetryingInboundTransportDecorator(
                inbound, 
                outbound, 
                options);
        }

        IOutboundTransport ITransportFactory.CreateOutboundTransport()
        {
            return CreateOutboundTransport();
        }

        protected abstract IInboundTransport CreateInboundTransport(string endpoint);

        protected abstract IOutboundTransport CreateOutboundTransport();
    }
}