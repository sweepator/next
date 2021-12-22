using Next.Abstractions.Serialization.Metadata;
using Next.Cqrs.Serialization;

namespace Next.Application.Cqrs.Extensions
{
    public static class SerializerMetadataProviderBuilderExtensions
    {
        public static ISerializerMetadataProviderBuilder IgnoreBaseCommandResponseProperties(this ISerializerMetadataProviderBuilder builder)
        {
            builder.AddProfile(new IgnoreBaseCommandResponseProperties());
            return builder;
        }
        
        public static ISerializerMetadataProviderBuilder IgnoreBaseAggregateCommandRequestProperties(this ISerializerMetadataProviderBuilder builder)
        {
            builder.AddProfile(new IgnoreBaseCommandRequestProperties());
            return builder;
        }
    }
}