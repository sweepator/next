using System;

namespace Next.Core.Metadata
{
    public class MetadataKeyNotFoundException : Exception
    {
        public MetadataKeyNotFoundException(string key)
            : base($"Could not find metadata key '{key}'")
        {
        }
    }
}