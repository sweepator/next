using System.Threading;
using System.Threading.Tasks;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelFactory<TProjectionModel>
        where TProjectionModel : IProjectionModel
    {
        Task<TProjectionModel> Create(
            object id, 
            CancellationToken cancellationToken = default);
    }
}