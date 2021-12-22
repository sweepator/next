
using Next.Abstractions.Domain.Serialization;

namespace System.Text.Json
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddDomainConverters(this JsonSerializerOptions jsonSerializerOptions)
        {
            jsonSerializerOptions.Converters.Add(new SingleValueObjectConverterFactory());
            return jsonSerializerOptions;
        }
    }
}