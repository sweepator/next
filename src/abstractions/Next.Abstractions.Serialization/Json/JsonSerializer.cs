using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Next.Abstractions.Serialization.Json
{
    public class JsonSerializer : IJsonSerializer, IJsonSerializerAsync
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer(JsonSerializerOptions options = null)
        {
            _options = options ?? JsonSerializerDefaults.GetDefaultSettings();
        }

        public JsonSerializer(IEnumerable<JsonConverter> jsonConverters)
            : this(JsonSerializerDefaults.GetDefaultSettings(jsonConverters))
        {
        }

        public T Deserialize<T>(TextReader input)
        {
            return (T)Deserialize(typeof(T), input);
        }

        public string Serialize(object input)
        {
            using var buffer = new StringWriter();
            
            Serialize(input, buffer);
            return buffer.ToString();
        }

        public void Serialize(
            object input,
            TextWriter output)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(input, input.GetType(), _options);
            output.Write(json);
        }

        public async Task SerializeAsync(
            object input, 
            Stream stream)
        {
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, input, _options);
        }

        public async Task<T> DeserializeAsync<T>(Stream stream)
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, _options);
        }

        public async Task<object> DeserializeAsync(
            Type type, 
            Stream stream)
        {
            return await System.Text.Json.JsonSerializer.DeserializeAsync(stream, type, _options);
        }
        
        public T Deserialize<T>(string json)
        {
            using var stream = new StringReader(json);
            return Deserialize<T>(stream);
        }

        public object Deserialize(
            Type type,
            string json)
        {
            using var stream = new StringReader(json);
            return Deserialize(type, stream);
        }
        
        public object Deserialize(
            Type type, 
            TextReader input)
        {
            var json = input.ReadToEnd();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize(json, type, _options);
            }
            catch (JsonException ex)
            {
                //check if it is a json path deserialization error
                if (!string.IsNullOrEmpty(ex.Path))
                {
                    throw new FormatException(ex.Path.Substring(ex.Path.LastIndexOf(".", StringComparison.Ordinal) + 1), ex);
                }

                throw;
            }
        }
    }
}
