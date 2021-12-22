using System;

namespace Next.Abstractions.Bus
{
    public interface IMessageSerializer
    {
        string Serialize(object message);
        object Deserialize(
            Type type, 
            string message);
    }
}