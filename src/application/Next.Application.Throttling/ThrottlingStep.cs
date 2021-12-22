using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Next.Application.Pipelines;
using Next.Core;
using Next.Core.Errors;
using Next.Cqrs.Commands;

namespace Next.Application.Throttling
{
    public class ThrottlingStep<TRequest, TResponse> : PipelineStep<TRequest, TResponse>
        where TRequest : ICommand<TResponse>, IIdentityRequest
        where TResponse : ICommandResponse, new()
    {
        private static readonly Guid ThrottlingNameSpaceId = Guid.Parse("12c30312-2080-4b37-b293-efbce01bc62f");
        private static readonly Error ThrottlingError = new Error(nameof(ThrottlingError));
        
        private readonly IIdentityRequestCache _identityRequestCache;
        private readonly ThrottlingStepOptions _throttlingOptions;

        public ThrottlingStep(
            IIdentityRequestCache identityRequestCache,
            IOptionsSnapshot<ThrottlingStepOptions> throttlingOptions)
        {
            _identityRequestCache = identityRequestCache;
            _throttlingOptions = throttlingOptions.Get(typeof(TRequest).Name);
        }
        
        public override async Task<TResponse> Execute(
            TRequest request, 
            IOperationContext context, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            if (!_throttlingOptions.IsEnabled)
            {
                return await next();
            }
            
            var bytes = request.GeIdentityComponents().SelectMany(b => b).ToArray();
            var id = GuidFactory.Deterministic.Create(
                ThrottlingNameSpaceId,
                bytes);

            var result = await _identityRequestCache.SetId(id, _throttlingOptions.Expiration);

            if (!result)
            {
                var response = new TResponse();
                response.AddError(ThrottlingError);
                return response;
            }
            
            return await next();
        }
    }
}