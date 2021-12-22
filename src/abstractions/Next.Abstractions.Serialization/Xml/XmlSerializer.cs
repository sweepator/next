using System.IO;

namespace Next.Abstractions.Serialization.Xml
{
    public class XmlSerializer : IXmlSerializer
    {
        public T Deserialize<T>(Stream stream)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var result = (T)serializer.Deserialize(stream);
            return result;
        }

        public T Deserialize<T>(string xml)
        {
            using (TextReader reader = new StringReader(xml))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                var result = (T)serializer.Deserialize(reader);
                return result;
            }
        }
    }
}
