using Next.Abstractions.Serialization.Metadata;
using Next.Web.Serialization.Json;

namespace System.Text.Json
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddMetadataProvider(
            this JsonSerializerOptions jsonSerializerOptions,
            Action<ISerializerMetadataProviderBuilder> setup)
        {
            var builder = new SerializerMetadataProviderBuilder();
            setup?.Invoke(builder);
            jsonSerializerOptions.Converters.Add(new JsonSerializationMetadataConverterFactory(builder.SerializerMetadataProvider));
            return jsonSerializerOptions;
        }
        
        public static JsonSerializerOptions AddMetadataProvider(
            this JsonSerializerOptions jsonSerializerOptions,
            ISerializerMetadataProvider serializerMetadataProvider)
        {
            jsonSerializerOptions.Converters.Add(new JsonSerializationMetadataConverterFactory(serializerMetadataProvider));
            return jsonSerializerOptions;
        }
    }
}