using System.Collections.Generic;
using Next.Abstractions.Bus.Transport;

namespace Next.Bus.Kafka.Transport
{
    public interface ITopicNamingStrategy
    {
        string GetProducerTopic(TransportMessage transportMessage);
        
        IEnumerable<string> GetConsumerTopics();

        string GetConsumerTopic(string endpoint);
    }
}