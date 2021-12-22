using Next.Application.Contracts;
using Next.Application.Pipelines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Application.Log
{
    public class LogStep<TRequest, TResponse> : PipelineStep<TRequest, TResponse>
         where TRequest : IRequest<TResponse>
         where TResponse : IResponse
    {
        private readonly ILogStepSerializer _logStepSerializer;
        private readonly ILogger _logger;
        private readonly LogStepOptions _logOptions;

        public LogStep(
            ILoggerFactory loggerFactory,
            ILogStepSerializer logStepSerializer,
            IOptionsSnapshot<LogStepOptions> logOptions)
        {
            _logStepSerializer = logStepSerializer;
            _logger = loggerFactory.CreateLogger($"LogStep<{typeof(TRequest).Name},{typeof(TResponse).Name}>");
            _logOptions = logOptions.Get(typeof(TRequest).Name);
        }

        public override async Task<TResponse> Execute(
            TRequest request,
            IOperationContext context,
            MediatR.RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            if (!_logOptions.IsEnabled)
            {
                return await next();
            }

            var actionName = request.GetActionName();

            try
            {
                var stopWatch = Stopwatch.StartNew();
                using var scope = _logger.NewScope(actionName);

                if (_logOptions.LogRequest)
                {
                    _logger.Info("Application request {0}", _logStepSerializer.Serialize(request));
                }

                var response = await next();
                stopWatch.Stop();
                
                if (_logOptions.LogResponse)
                {
                    _logger.Info("Application response {0} in {1}ms", 
                        _logStepSerializer.Serialize(response),
                        stopWatch.Elapsed.TotalMilliseconds);
                }

                scope.Complete();
                return response;
            }
            catch (Exception ex)
            {
                if (_logOptions.LogException)
                {
                    _logger.Error(ex, $"Error on processing request {actionName}.");
                }

                throw;
            }
        }
    }
}
