namespace Next.Application.Log
{
    public interface ILogStepSerializer
    {
        string Serialize(object obj);
    }
}