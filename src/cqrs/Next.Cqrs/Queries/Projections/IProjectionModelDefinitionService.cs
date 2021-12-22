using Next.Abstractions.Core.Versioning;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelDefinitionService : IVersionedTypeDefinitionService<ProjectionModelVersionAttribute, ProjectionModelDefinition>
    {

    }
}