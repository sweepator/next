namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelContextFactory
    {
        IProjectionModelContext Create(
            object id, 
            bool isNew);
    }
}