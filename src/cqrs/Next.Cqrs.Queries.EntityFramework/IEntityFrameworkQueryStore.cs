using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.EntityFramework
{
    public interface IEntityFrameworkQueryStore<TProjectionModel> : IQueryableQueryStore<TProjectionModel> 
        where TProjectionModel : class, IProjectionModel
    {
        
    }
}