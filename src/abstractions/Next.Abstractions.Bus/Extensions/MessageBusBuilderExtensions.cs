using Next.Abstractions.Bus.Memory.Subscriptions;
using Next.Abstractions.Bus.Memory.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusBuilderExtensions
    {
        public static IMessageBusBuilder WithInMemory(this IMessageBusBuilder messageBusBuilder)
        {
            return messageBusBuilder
                .WithInMemorySubscriptionBroker()
                .WithInMemorySubscriptionStore()
                .WithInMemoryTransport();
        }
        
        public static IMessageBusBuilder WithInMemorySubscriptionBroker(this IMessageBusBuilder messageBusBuilder)
        {
            return messageBusBuilder.WithCustomSubscriptionBroker(new InMemorySubscriptionBroker());
        }
        
        public static IMessageBusBuilder WithInMemorySubscriptionStore(this IMessageBusBuilder messageBusBuilder)
        {
            return messageBusBuilder.WithCustomSubscriptionStore(new InMemorySubscriptionStore());
        }
        
        public static IMessageBusBuilder WithInMemoryTransport(this IMessageBusBuilder messageBusBuilder)
        {
            return messageBusBuilder.WithCustomTransport(new InMemoryTransportFactory());
        }
    }
}
