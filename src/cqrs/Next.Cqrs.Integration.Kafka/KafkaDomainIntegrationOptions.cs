namespace Next.Cqrs.Integration.Kafka
{
    public class KafkaDomainIntegrationOptions<TIntegrationEvent>
        where TIntegrationEvent : class
    {
        private const string DefaultNamespace = "next.integration";

        public string Topic { get; set; } = $"{DefaultNamespace}.{typeof(TIntegrationEvent).Name.ToLower()}";

        public string BootstrapServers { get; set; }
        public bool AutoCreateTopic { get; set; }
    }
}