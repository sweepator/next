using System.Collections.Generic;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Represents an immutable generic message with support for headers, name and a payload
    /// </summary>
    public class Message
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();
        
        public object Payload { get; }
        
        public string Name { get; }
        
        public IReadOnlyDictionary<string, string> Headers { get; }

        public Message(
            object payload, 
            string name, 
            Dictionary<string, string> headers = null)
        {
            Payload = payload;
            Name = name;
            Headers = headers ?? EmptyHeaders;
        }
    }
}