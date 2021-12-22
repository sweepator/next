using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Trace
{
    internal sealed class TraceInfoProvider : ITraceInfoProvider
    {
        private readonly IEnumerable<ITraceStrategy> _traceStrategies;

        public TraceInfoProvider(IEnumerable<ITraceStrategy> traceStrategies)
        {
            _traceStrategies = traceStrategies;
        }

        public TraceInfo GetTraceInfo()
        {
            return _traceStrategies?
                    .Select(s => s.GetTraceInfo())
                    .Where(s => s != null)
                    .FirstOrDefault();
        }
    }
}
