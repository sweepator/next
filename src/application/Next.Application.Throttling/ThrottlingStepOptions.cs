using System;
using Next.Application.Pipelines;

namespace Next.Application.Throttling
{
    public class ThrottlingStepOptions : PipelineStepOptions
    {
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}
