using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Next.Abstractions.Serialization.Metadata;

namespace Next.Web.Serialization.Json
{
    internal class JsonSerializationMetadataConverterFactory : JsonConverterFactory
    {
        private readonly ISerializerMetadataProvider _serializerMetadataProvider;

        public JsonSerializationMetadataConverterFactory(ISerializerMetadataProvider serializerMetadataProvider)
        {
            _serializerMetadataProvider = serializerMetadataProvider;
        }
        
        public override bool CanConvert(Type typeToConvert)
        {
            var canConvert = typeToConvert.IsClass && _serializerMetadataProvider.CanApply(typeToConvert);
            return canConvert;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeof(JsonSerializationMetadataConverter<>).MakeGenericType(typeToConvert);
            var converter = (JsonConverter)Activator.CreateInstance(type, _serializerMetadataProvider);
            return converter;
        }
    }
}