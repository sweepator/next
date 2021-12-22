using System;
using System.Collections.Generic;
using Next.Abstractions.Core.Versioning;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SnapshotDefinitionService :
        VersionedTypeDefinitionService<IState, SnapshotVersionAttribute, SnapshotVersionDefinition>,
        ISnapshotDefinitionService
    {
        public SnapshotDefinitionService(IEnumerable<Type> loadedVersionedTypes)
        {
            Load(loadedVersionedTypes);
        }

        protected override SnapshotVersionDefinition CreateDefinition(
            int version, 
            Type type, 
            string name)
        {
            return new(version, type, name);
        }
    }
}