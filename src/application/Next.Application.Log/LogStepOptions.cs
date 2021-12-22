using Next.Application.Pipelines;

namespace Next.Application.Log
{
    public class LogStepOptions : PipelineStepOptions
    {
        public bool LogRequest { get; set; } = true;
        public bool LogResponse { get; set; } = true;
        public bool LogException { get; set; } = true;
    }
}
