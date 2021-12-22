using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.Memory
{
    public interface IInMemoryQueryStore<TProjectionModel> : IQueryableQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
    }
}