using System;

namespace Next.Bus.MassTransit.Configuration
{
    public interface IMassTransitBusConfigurationBuilder
    {
        IMassTransitBusConfigurationBuilder ConsumerTimeout(TimeSpan timeSpan);
    }
}