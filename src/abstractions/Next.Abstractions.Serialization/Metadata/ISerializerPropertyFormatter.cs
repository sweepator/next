namespace Next.Abstractions.Serialization.Metadata
{
    public interface ISerializerPropertyFormatter
    {
        string Format(object instance);
    }
    
    public interface ISerializerPropertyFormatter<in T>: ISerializerPropertyFormatter
        where T: class
    {
        string Format(T instance);
    }
}