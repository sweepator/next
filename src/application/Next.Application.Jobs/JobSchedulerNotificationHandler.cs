using System;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Jobs;
using Next.Abstractions.Mapper;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Application.Jobs
{
    public sealed class JobSchedulerNotificationHandler<TNotification, TJob, TJobRequest> : NotificationHandler<TNotification>
        where TNotification : INotification
        where TJob : IJob<TJobRequest>
        where TJobRequest: IJobRequest
    {
        private readonly IMapper _mapper;
        private readonly IJobService _jobService;

        public JobSchedulerNotificationHandler(
            IMapper mapper,
            IJobService jobService)
        {
            _mapper = mapper;
            _jobService = jobService;
        }
        
        public override Task Execute(
            TNotification notification,
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            var jobRequest = _mapper.Map<TJobRequest>(notification);
            
            _jobService.Schedule<TJob, TJobRequest>(
                jobRequest, 
                TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }
    }
}