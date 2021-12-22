using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelDefinition : VersionedTypeDefinition
    {
        public ProjectionModelDefinition(
            int version,
            Type type,
            string name)
            : base(version, type, name)
        {
        }
    }
}