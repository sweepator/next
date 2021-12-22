using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.Domain
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = true
    )]
    public class AggregateEventVersionAttribute : VersionedTypeAttribute
    {
        public AggregateEventVersionAttribute(string name, int version)
            : base(name, version)
        {
        }
    }
}