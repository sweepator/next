namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Contains headers used internally when sending and receiving messages
    /// </summary>
    public class MessageHeaders
    {
        /// <summary>
        /// Identifies the ID of the message.
        /// </summary>
        public const string Id = "next.id";

        /// <summary>
        /// Identifies the endpoint of the destination of the message. 
        /// In queue based endpoints, it will be the name of the queue.
        /// </summary>
        public const string Endpoint = "next.endpoint";

        /// <summary>
        /// Identifies the component that handles the message once it arrives at its destination.
        /// Typically this is the name of a message handler.
        /// </summary>
        public const string Component = "next.component";

        /// <summary>
        /// Identifies the original endpoint of the message. 
        /// This is used when messages are dead lettered
        /// </summary>
        public const string OriginalEndpoint = "next.original_endpoint";
    }
}