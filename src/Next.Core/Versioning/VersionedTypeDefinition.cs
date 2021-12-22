using System;

namespace Next.Abstractions.Core.Versioning
{
    public abstract class VersionedTypeDefinition
    {
        public int Version { get; }
        public Type Type { get; }
        public string Name { get; }

        protected VersionedTypeDefinition(
            int version,
            Type type,
            string name)
        {
            Version = version;
            Type = type;
            Name = name;
        }
    }
}