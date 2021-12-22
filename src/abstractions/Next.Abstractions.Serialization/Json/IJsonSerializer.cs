using System;

namespace Next.Abstractions.Serialization.Json
{
    public interface IJsonSerializer
    {
        string Serialize(object input);
        T Deserialize<T>(string json);
        object Deserialize(
            Type type,
            string json);
    }
}
