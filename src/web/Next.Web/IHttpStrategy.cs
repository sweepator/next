namespace Next.Web
{
    public interface IHttpStrategy<T>
    {
        T GetHttpStrategyObject();
    }
}