namespace Next.Cqrs.Queries
{
    public interface IQueryStoreContext<out TContext>
    {
        TContext Context { get; }
    }
}