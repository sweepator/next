using Next.Abstractions.Jobs;
using System.Threading.Tasks;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Application.Jobs
{
    public class PipelineJob<TJobRequest, TRequest, TResponse> : Job<TJobRequest>
        where TJobRequest : IJobRequest<TRequest>
        where TRequest : class, IRequest<TResponse>
        where TResponse : IResponse
    {
        private readonly IPipelineEngine _pipelineEngine;

        protected PipelineJob(IPipelineEngine pipelineEngine)
        {
            _pipelineEngine = pipelineEngine;
        }

        public override async Task Run(TJobRequest jobRequest)
        {
            var response = await _pipelineEngine.Execute(jobRequest.Content);

            /*if (!commandResponse.IsSuccess)
            {
                if (ThrowExceptionOnResponseWithErrors)
                {
                    throw new PipeLineOperationException(commandResponse.Errors);
                }
            }*/
        }
    }
}
