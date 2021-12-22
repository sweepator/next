using System;
using Next.Abstractions.Core.Versioning;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SnapshotVersionAttribute : VersionedTypeAttribute
    {
        public SnapshotVersionAttribute(string name, int version)
            : base(name, version)
        {
        }
    }
}