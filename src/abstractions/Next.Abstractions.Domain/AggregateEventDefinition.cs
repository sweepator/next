using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.Domain
{
    public class AggregateEventDefinition : VersionedTypeDefinition
    {
        public AggregateEventDefinition(
            int version,
            Type type,
            string name)
            : base(version, type, name)
        {
        }
    }
}