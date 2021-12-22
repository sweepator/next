using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SnapshotVersionDefinition : VersionedTypeDefinition
    {
        public SnapshotVersionDefinition(
            int version,
            Type type,
            string name)
            : base(version, type, name)
        {
        }
    }
}