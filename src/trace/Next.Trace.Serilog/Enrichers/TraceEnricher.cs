using Serilog.Core;
using Serilog.Events;
using Next.Abstractions.Trace;

namespace Next.Trace.Serilog.Enrichers
{
    public class TraceEnricher : ILogEventEnricher
    {
        private readonly ITraceInfoProvider _traceInfoProvider;

        public TraceEnricher(ITraceInfoProvider traceInfoProvider)
        {
            _traceInfoProvider = traceInfoProvider;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var traceInfo = _traceInfoProvider.GetTraceInfo();

            if (traceInfo != null)
            {
                if (!string.IsNullOrEmpty(traceInfo?.RequestId))
                {
                    logEvent.AddPropertyIfAbsent(
                        propertyFactory.CreateProperty(nameof(TraceInfo.RequestId),
                        traceInfo?.RequestId));
                }

                if (!string.IsNullOrEmpty(traceInfo?.CorrelationId))
                {
                    logEvent.AddPropertyIfAbsent(
                        propertyFactory.CreateProperty(nameof(TraceInfo.CorrelationId),
                        traceInfo?.CorrelationId));
                }
            }
        }
    }
}
