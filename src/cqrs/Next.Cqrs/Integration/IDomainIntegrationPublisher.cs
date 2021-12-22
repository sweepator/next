using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Integration
{
    public interface IDomainIntegrationPublisher<in TIntegrationEvent>
        where TIntegrationEvent: class
    {
        Task Publish(
            TIntegrationEvent integrationEvent,
            IMetadata metadata);
    }
}