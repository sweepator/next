using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Abstractions.Domain.Serialization
{
    public class SingleValueObjectConverter: JsonConverter<ISingleValueObject>
    {
        private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumentTypes = new();
        
        public override ISingleValueObject Read(
            ref Utf8JsonReader reader, 
            Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var parameterType = ConstructorArgumentTypes.GetOrAdd(
                typeToConvert,
                t =>
                {
                    var constructorInfo = t.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
                    var parameterInfo = constructorInfo.GetParameters().Single();
                    return parameterInfo.ParameterType;
                });

            var value = JsonSerializer.Deserialize(ref reader, parameterType);
            return (ISingleValueObject)Activator.CreateInstance(typeToConvert, value);
        }

        public override void Write(
            Utf8JsonWriter writer, 
            ISingleValueObject singleValueObject, 
            JsonSerializerOptions options)
        {
            var value = singleValueObject.GetValue();
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}