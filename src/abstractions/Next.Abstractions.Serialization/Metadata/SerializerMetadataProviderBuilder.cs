using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Next.Abstractions.Serialization.Metadata
{
    public sealed class SerializerMetadataProviderBuilder : ISerializerMetadataProviderBuilder
    {
        private readonly List<ISerializerMetadataProfile> _profiles = new List<ISerializerMetadataProfile>();

        public ISerializerMetadataProvider SerializerMetadataProvider => new SerializerMetadataProvider(_profiles);

        public ISerializerMetadataProviderBuilder AddProfile(ISerializerMetadataProfile serializerMetadataProfile)
        {
            _profiles.Add(serializerMetadataProfile);
            return this;
        }

        public ISerializerMetadataProviderBuilder AddProfilesFrom(params Assembly[] assemblies)
        {
            _profiles.AddRange(assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && typeof(ISerializerMetadataProfile).IsAssignableFrom(t))
                .Select(t => (ISerializerMetadataProfile) Activator.CreateInstance(t, Array.Empty<object>()))
                .ToArray());

            return this;
        }

        public ISerializerMetadataProviderBuilder For<T>(Action<ISerializerMetadataProfileBuilder<T>> setup)
            where T : class
        {
            var serializerMetadataProfileBuilder = new SerializerMetadataProfileBuilder<T>();
            setup?.Invoke(serializerMetadataProfileBuilder);
            _profiles.Add(serializerMetadataProfileBuilder.SerializerMetadataProfile);

            return this;
        }
    }
}