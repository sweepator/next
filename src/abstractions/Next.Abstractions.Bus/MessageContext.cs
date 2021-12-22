using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Context a message being processed.
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// Gets the message serializer
        /// </summary>
        public IMessageSerializer MessageSerializer { get; }

        /// <summary>
        /// Gets the original TransportMessage
        /// </summary>
        public TransportMessage Message { get; }
        
        public MessageContext(
            IMessageSerializer messageSerializer,
            TransportMessage message)
        {
            MessageSerializer = messageSerializer;
            Message = message;
        }

        public TMessage GetMessage<TMessage>()
        {
            return (TMessage) MessageSerializer.Deserialize(
                typeof(TMessage),
                Message.PayLoad);
        }
    }
}