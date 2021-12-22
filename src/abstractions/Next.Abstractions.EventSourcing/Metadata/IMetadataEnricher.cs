using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Metadata
{
    public interface IMetadataEnricher
    {
        void Enrich(IDomainEvent domainEvent);
    }
}