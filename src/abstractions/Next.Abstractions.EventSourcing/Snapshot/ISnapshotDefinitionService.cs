using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISnapshotDefinitionService : IVersionedTypeDefinitionService<SnapshotVersionAttribute, SnapshotVersionDefinition>
    {
    }
}