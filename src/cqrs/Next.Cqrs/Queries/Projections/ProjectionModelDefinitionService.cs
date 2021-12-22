using System;
using System.Collections.Generic;
using Next.Abstractions.Core.Versioning;

namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelDefinitionService :
        VersionedTypeDefinitionService<IProjectionModel, ProjectionModelVersionAttribute, ProjectionModelDefinition>,
        IProjectionModelDefinitionService
    {
        public ProjectionModelDefinitionService(IEnumerable<Type> loadedVersionedTypes)
        {
            Load(loadedVersionedTypes);
        }

        protected override ProjectionModelDefinition CreateDefinition(
            int version, 
            Type type, 
            string name)
        {
            return new ProjectionModelDefinition(version, type, name);
        }
    }
}