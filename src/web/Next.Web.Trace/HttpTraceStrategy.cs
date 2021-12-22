using Next.Abstractions.Trace;
using Microsoft.AspNetCore.Http;

namespace Next.Web.Trace
{
    public class HttpTraceStrategy : HttpStrategy<TraceInfo>, ITraceStrategy
    {
        public HttpTraceStrategy(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        public TraceInfo GetTraceInfo()
        {
            return this.GetHttpStrategyObject();
        }

        protected override TraceInfo InternalGetStrategyValue(HttpContext httpContext)
        {
             var requestId = httpContext.GetRequestId();
            var journeyId = httpContext.GetJourneyId() ?? requestId;

            return new TraceInfo(requestId, journeyId);
        }
    }
}
