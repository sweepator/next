using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Next.Cqrs.Commands;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Web.Application.PortAdapters
{
    public interface IHttpPortAdapter
    {
        public Task<IActionResult> Execute<TCommandResponse>(
            ICommand<TCommandResponse> command,
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse;
        
        public Task<IActionResult> Send<TCommandResponse>(
            ICommand<TCommandResponse> command,
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse;

        public Task<IActionResult> Execute<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel : IProjectionModel;
        
        public Task<IActionResult> ExecuteSingle<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel : IProjectionModel;
    }
}