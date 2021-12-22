using System.Linq;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpExtensions
    {
        private const string CorrelationId = "X-CorrelationId";
        public static string GetRequestId(this HttpContext httpContext)
        {
            return httpContext.TraceIdentifier;
        }

        public static string GetJourneyId(this HttpContext httpContext)
        {
            var journeyId = httpContext.Request.Headers
                .TryGetValue(CorrelationId, out var values)
                ? values.FirstOrDefault()
                : null;

            return journeyId;
        }
    }
}
