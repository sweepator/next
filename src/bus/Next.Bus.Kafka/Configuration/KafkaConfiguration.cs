using System;
using Next.Bus.Kafka.Transport;

namespace Next.Bus.Kafka.Configuration
{
    public class KafkaConfiguration
    {
        public const string NameSpace = "next.internal";
        
        public string BootstrapServers { get; set; }
        
        public string ConsumerGroupId { get; set; }
        
        public string ConsumerClientId { get; set; }
        
        public string ProducerClientId { get; set; }

        public TimeSpan ErrorCheckLock { get; set; } = TimeSpan.FromSeconds(5);
        
        public TimeSpan ConsumerTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
        
        public TimeSpan ErrorConsumerTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}