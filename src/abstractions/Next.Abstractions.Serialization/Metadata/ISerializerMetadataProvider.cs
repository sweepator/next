using System;

namespace Next.Abstractions.Serialization.Metadata
{
    public interface ISerializerMetadataProvider
    {
        bool CanApply(Type type);
        
        bool CanExcludeProperty(
            Type type, 
            string propertyName);
        
        string GetPropertyName(
            Type type, 
            string propertyName);

        string GetFormattedStringValue(
            object targetInstance, 
            string currentValue,
            string propertyName);
    }
}