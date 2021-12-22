using System;

namespace Next.Bus.MassTransit.Configuration
{
    public class MassTransitBusConfiguration
    {
        public const string NameSpace = "next.internal";
        
        public TimeSpan ConsumerTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}