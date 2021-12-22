using System;

namespace Next.Bus.MassTransit.Configuration
{
    internal class MassTransitBusConfigurationBuilder : IMassTransitBusConfigurationBuilder
    {
        private readonly MassTransitBusConfiguration _busConfiguration;

        internal MassTransitBusConfigurationBuilder(MassTransitBusConfiguration busConfiguration)
        {
            _busConfiguration = busConfiguration;
        }
        
        public IMassTransitBusConfigurationBuilder ConsumerTimeout(TimeSpan timeSpan)
        {
            _busConfiguration.ConsumerTimeout = timeSpan;
            return this;
        }
    }
}