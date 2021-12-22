using System;
using System.Threading.Tasks;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IQueryStorePopulator
    {
        Task Purge<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel;

        Task Populate<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel;

        Task Populate(Type projectionModelType);

        Task Purge(Type projectionModelType);

        Task Delete(
            object id,
            Type projectionModelType);
    }
}