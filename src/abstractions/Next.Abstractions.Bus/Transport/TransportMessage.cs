using System.Collections.Generic;

namespace Next.Abstractions.Bus.Transport
{
    /// <summary>
    /// Represents a message on the transport level, already serialized, ready to be sent or processed.
    /// </summary>
    public class TransportMessage
    {
        /// <summary>
        /// Id of the message
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the message. Usually it's the actual Type of the payload object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Body of the message. Usually it's the serialized form of the payload object.
        /// </summary>
        public string PayLoad { get; set; }

        /// <summary>
        /// Headers included in the message. See <see cref="Next.Abstractions.Bus.MessageHeaders"/>
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        
        public TransportMessage()
        {
            Headers = new Dictionary<string, string>();
        }
    }
}