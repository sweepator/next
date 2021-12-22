using Next.Application.Contracts;
using Next.Application.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Next.Web.Application.Pipelines
{
    internal class HttpContextBinderStep<TRequest, TResponse> : PipelineStep<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
         where TResponse : IResponse
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextBinderStep(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<TResponse> Execute(
            TRequest request,
            IOperationContext context,
            MediatR.RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            var response = await next();
            
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext?.Features.Set(context);

            return response;
        }
    }
}
