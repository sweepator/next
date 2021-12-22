using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.Domain
{
    public interface IAggregateEventDefinitionService : IVersionedTypeDefinitionService<AggregateEventVersionAttribute, AggregateEventDefinition>
    {
    }
}