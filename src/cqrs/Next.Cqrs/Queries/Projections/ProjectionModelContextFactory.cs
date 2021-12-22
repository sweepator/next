namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelContextFactory : IProjectionModelContextFactory
    {
        public IProjectionModelContext Create(
            object id, 
            bool isNew)
        {
            return new ProjectionModelContext(
                id, 
                isNew);
        }
    }
}