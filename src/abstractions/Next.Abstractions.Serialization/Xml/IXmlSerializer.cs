using System.IO;

namespace Next.Abstractions.Serialization.Xml
{
    public interface IXmlSerializer
    {
        T Deserialize<T>(Stream stream);
        T Deserialize<T>(string xml);
    }
}
