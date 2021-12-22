using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Integration
{
    public interface IDomainIntegration
    {
        Task Publish(IDomainEvent domainEvent);
    }
    
    public interface IDomainIntegration<in TAggregate, in TIdentity, in TAggregateEvent, in TIntegrationEvent> 
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
        where TIntegrationEvent: class
    {
        Task Publish(
            TIntegrationEvent integrationEvent, 
            IMetadata metadata);
    }
}