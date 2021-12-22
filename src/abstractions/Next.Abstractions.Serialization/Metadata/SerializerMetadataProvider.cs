using System;
using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Serialization.Metadata
{
    public class SerializerMetadataProvider: ISerializerMetadataProvider
    {
        private readonly IEnumerable<ISerializerMetadataProfile> _profiles;

        public SerializerMetadataProvider()
        {
            _profiles = new List<ISerializerMetadataProfile>();
        }
        
        public SerializerMetadataProvider(IEnumerable<ISerializerMetadataProfile> profiles)
        {
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        }

        public bool CanApply(Type type)
        {
            var hasIgnoredProperties = _profiles.SelectMany(p => p.GetIgnoredProperties(type)).Any();
            var hasMappedProperties = _profiles.SelectMany(p => p.GetPropertyNames(type)).Any();
            var hasFormatters = _profiles.SelectMany(p => p.GetPropertyFormatters(type)).Any();

            return hasIgnoredProperties || hasMappedProperties || hasFormatters;
        }

        public bool CanExcludeProperty(
            Type type, 
            string propertyName)
        {
            foreach (var profile in _profiles)
            {
                var ignoredProperties = profile.GetIgnoredProperties(type);

                if (ignoredProperties.Any(o=>o.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        public string GetPropertyName(
            Type type, 
            string propertyName)
        {
            foreach (var profile in _profiles)
            {
                var propertyNames = profile.GetPropertyNames(type);

                if (propertyNames.ContainsKey(propertyName))
                {
                    return propertyNames[propertyName];
                }
            }
            
            return propertyName;
        }

        public string GetFormattedStringValue(
            object targetInstance, 
            string currentValue,
            string propertyName)
        {
            foreach (var profile in _profiles)
            {
                var propertyFormatters = profile.GetPropertyFormatters(targetInstance.GetType());

                if (propertyFormatters.ContainsKey(propertyName))
                {
                    var formatter = propertyFormatters[propertyName];
                    var formattedValue = formatter.Format(targetInstance);
                    return formattedValue;
                }
            }

            return currentValue;
        }
    }
}