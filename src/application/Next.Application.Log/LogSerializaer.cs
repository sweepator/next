using Next.Abstractions.Serialization.Json;

namespace Next.Application.Log
{
    internal class LogSerializer: ILogStepSerializer
    {
        private readonly IJsonSerializer _jsonSerializer;

        public LogSerializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }
        
        public string Serialize(object obj)
        {
            return _jsonSerializer.Serialize(obj);
        }
    }
}