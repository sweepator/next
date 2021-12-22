namespace Next.Abstractions.Bus.Transport
{
    public class InboundTransportOptions
    {
        private const string DeadLetterEndpointFormat = "{0}-deadletter";
        public const int DefaultMaxDeliveryCount = 10;
        public const int DefaultConcurrencyLevel  = 10;
        internal const bool DefaultDeadLetterMessages = true;
        
        public string Endpoint { get; }
        public int MaxDeliveryCount { get; }
        public bool DeadLeterMessages { get; }

        public InboundTransportOptions(
            string endpoint, 
            int maxDeliveryCount = DefaultMaxDeliveryCount, 
            bool deadLetterMessages = DefaultDeadLetterMessages)
        {
            Endpoint = endpoint;
            MaxDeliveryCount = maxDeliveryCount;
            DeadLeterMessages = deadLetterMessages;
        }

        public string GetDeadLetterEndpoint()
        {
            return string.Format(DeadLetterEndpointFormat, Endpoint);
        }
    }
}