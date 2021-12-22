namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModel
    {
        string Id { get; }
        int? Version { get; set; }
    }
}