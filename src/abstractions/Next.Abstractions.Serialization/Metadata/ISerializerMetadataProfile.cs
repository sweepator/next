using System;
using System.Collections.Generic;

namespace Next.Abstractions.Serialization.Metadata
{
    public interface ISerializerMetadataProfile
    {
        IEnumerable<string> GetIgnoredProperties(Type targetType);
        IDictionary<string, string> GetPropertyNames(Type targetType);
        IDictionary<string, ISerializerPropertyFormatter> GetPropertyFormatters(Type targetType);
    }
}