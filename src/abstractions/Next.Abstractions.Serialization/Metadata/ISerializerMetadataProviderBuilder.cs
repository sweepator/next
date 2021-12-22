using System;
using System.Reflection;

namespace Next.Abstractions.Serialization.Metadata
{
    public interface ISerializerMetadataProviderBuilder
    {
        ISerializerMetadataProvider SerializerMetadataProvider { get; }
        ISerializerMetadataProviderBuilder AddProfile(ISerializerMetadataProfile serializerMetadataProfile);
        ISerializerMetadataProviderBuilder AddProfilesFrom(params Assembly[] assemblies);
        ISerializerMetadataProviderBuilder For<T>(Action<ISerializerMetadataProfileBuilder<T>> setup)
            where T: class;
    }
}