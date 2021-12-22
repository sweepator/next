namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelContext
    {
        void MarkForDeletion();
        bool IsMarkedForDeletion { get; }
        object Id { get; }
        bool IsNew { get; }
    }
}