using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Next.Abstractions.Serialization.Metadata;

namespace Next.Web.Serialization.Json
{
    internal class JsonSerializationMetadataConverter<T> : JsonConverter<T>
    {
        private readonly ISerializerMetadataProvider _serializerMetadataProvider;
            
        public JsonSerializationMetadataConverter(ISerializerMetadataProvider serializerMetadataProvider)
        {
            _serializerMetadataProvider = serializerMetadataProvider;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var targetType = typeof(T);
            writer.WriteStartObject();

            using (var document = JsonDocument.Parse(JsonSerializer.Serialize(value)))
            {
                foreach (var property in document.RootElement.EnumerateObject().Where(property => !_serializerMetadataProvider.CanExcludeProperty(targetType, property.Name)))
                {
                    var propertyName = _serializerMetadataProvider.GetPropertyName(targetType, property.Name);
                    writer.WritePropertyName(options.PropertyNamingPolicy != null
                            ? options.PropertyNamingPolicy.ConvertName(propertyName)
                            : propertyName);

                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        var propertyInfo = typeof(T).GetProperty(property.Name);
                        var propertyValue = propertyInfo.GetValue(value);
                        JsonSerializer.Serialize(writer, propertyValue, options);
                    }
                    else 
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var currentValue = property.Value.GetString();
                        var val = _serializerMetadataProvider.GetFormattedStringValue(
                            value, 
                            currentValue,
                            property.Name);
                        
                        writer.WriteStringValue(val);
                    }
                    else
                    {
                        property.Value.WriteTo(writer);
                    }
                }
            }

            writer.WriteEndObject();
        }
    }
}