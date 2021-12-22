using System;
using Next.Abstractions.Bus;
using Next.Abstractions.Serialization.Json;

namespace Next.Bus.Json
{
    public class MessageJsonSerializer: IMessageSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;

        public MessageJsonSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }
        
        public string Serialize(object message)
        {
            return _jsonSerializer.Serialize(message);
        }

        public object Deserialize(
            Type type, 
            string message)
        {
            return _jsonSerializer.Deserialize(
                type,
                message);
        }
    }
}