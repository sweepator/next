namespace Next.Abstractions.Mapper
{
    public interface IMappable<out T>
    {
        T Map();
    }
}