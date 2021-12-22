using Next.Abstractions.Mapper;
using Next.Application.Contracts;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IQueryRequest<out TProjectionModel> : IRequest<IQueryResponse<TProjectionModel>>, IMappable<IQueryPredicate>
        where TProjectionModel: IProjectionModel
    {
    }
}   