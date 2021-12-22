using System;

namespace Next.Abstractions.Core.Versioning
{
    public abstract class VersionedTypeAttribute : Attribute
    {
        public string Name { get; }
        public int Version { get; }

        protected VersionedTypeAttribute(string name, int version)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be positive");
            }

            Name = name;
            Version = version;
        }
    }
}