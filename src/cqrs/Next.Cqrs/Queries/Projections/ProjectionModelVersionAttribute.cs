using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Cqrs.Queries.Projections
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true
    )]
    public class ProjectionModelVersionAttribute : VersionedTypeAttribute
    {
        public ProjectionModelVersionAttribute(
            string name, 
            int version)
            : base(name, version)
        {
        }
    }
}