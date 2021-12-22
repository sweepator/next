using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Abstractions.Domain.Serialization
{
    public class SingleValueObjectConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var canConvert = typeof(ISingleValueObject).IsAssignableFrom(typeToConvert);
            return canConvert;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new SingleValueObjectConverter();
        }
    }
}