namespace Next.Abstractions.Trace
{
    public class TraceInfo
    {
        public string RequestId { get; }

        public string CorrelationId { get; }

        public TraceInfo(string requestId, string correlationId)
        {
            RequestId = requestId;
            CorrelationId = correlationId;
        }
    }
}
