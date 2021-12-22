namespace Next.Abstractions.Bus.Subscriptions
{
    /// <summary>
    /// Subscription information for a specific message.
    /// </summary>
    public class Subscription
    {
        private static readonly char[] Separator = new char[] { '/' };
        
        /// <summary>
        /// Topic name. Usually the name of a message
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Endpoint name. Usually the logical name of a process
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Component name within the endpoint. Usually the name of the handler that subscribes the message.
        /// </summary>
        /// <remarks>
        /// If one endpoint has multiple handlers (multiple components), each will have its own subscription and receive a copy of the message.
        /// </remarks>
        public string Component { get; }

        public string Id { get; }
        
        public Subscription(
            string topic, 
            string endpoint, 
            string component)
        {
            Topic = topic;
            Endpoint = endpoint;
            Component = component;
            Id = $"{topic}/{endpoint}/{component}";
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Subscription other && Id.Equals(other.Id);
        }
        
        public static Subscription FromId(string id)
        {
            var parts = id.Split(Separator, 3);

            var topic = parts[0];
            var endpoint = parts[1];
            var component = parts[2];

            return new Subscription(topic, endpoint, component);
        }
    }
}