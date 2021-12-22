namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelContext : IProjectionModelContext
    {
        public bool IsMarkedForDeletion { get; private set; }
        public bool IsNew { get; }
        public object Id { get; }

        public ProjectionModelContext(
            object id,
            bool isNew)
        {
            Id = id;
            IsNew = isNew;
        }

        public void MarkForDeletion()
        {
            IsMarkedForDeletion = true;
        }
    }
}