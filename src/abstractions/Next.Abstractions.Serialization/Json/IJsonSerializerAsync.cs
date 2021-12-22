using System;
using System.IO;
using System.Threading.Tasks;

namespace Next.Abstractions.Serialization.Json
{
    public interface IJsonSerializerAsync
    {
        Task SerializeAsync(object input, Stream stream);
        Task<T> DeserializeAsync<T>(Stream stream);
        Task<object> DeserializeAsync(Type type, Stream stream);
    }
}