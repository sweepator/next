using Next.Abstractions.Bus.Configuration;
using Next.Cqrs.Configuration;

namespace Next.HomeBanking.Web.Api.Extensions
{
    public static class MessageBusBuilderExtensions
    {
        public static IMessageBusBuilder UseKafka(
            this IMessageBusBuilder messageBusBuilder,
            string bootstrapServers,
            IDomainMetadataInfo domainMetadataInfo)
        {
            return messageBusBuilder
                .WithKafkaTransport(o => o.WithBootstrapServers(bootstrapServers))
                .WithProcessorsByAggregateRoot(domainMetadataInfo);
        }
        
        public static IMessageBusBuilder UseMassTransit(
            this IMessageBusBuilder messageBusBuilder,
            IDomainMetadataInfo domainMetadataInfo)
        {
            return messageBusBuilder
                .WithMassTransitTransport()
                .WithProcessorsByAggregateRoot(domainMetadataInfo);
        }
    }
}