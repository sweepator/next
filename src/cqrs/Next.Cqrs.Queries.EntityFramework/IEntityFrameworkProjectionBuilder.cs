using System;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.EntityFramework
{
    public interface IEntityFrameworkProjectionBuilder
    {
        public IEntityFrameworkProjectionBuilder Projection<TProjectionModel>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;
    }
}