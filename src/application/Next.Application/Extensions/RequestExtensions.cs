namespace Next.Application.Contracts
{
    public static class RequestExtensions
    {
        private const string DefaultRequestSuffix = "Request";

        public static string GetActionName(this IRequest request)
        {
            return InternalGetActionName(request);
        }
        
        public static string GetActionName<TResponse>(this IRequest<TResponse> request)
            where TResponse: IResponse
        {
            return InternalGetActionName(request);
        }

        private static string InternalGetActionName(object request)
        {
            return request.GetType().Name
                .Replace("QueryRequest", string.Empty)
                .Replace("Command", string.Empty);
        }
    }
}
