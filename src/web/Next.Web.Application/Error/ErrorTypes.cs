namespace Next.Web.Application.Error
{
    public class ErrorTypes
    {
        public static string DefaultNamespace= "/api/errors/types";

        public static readonly string ServiceUnavailable = GetErrorType("service-unavailable");

        public static readonly string InvalidField = GetErrorType("invalid-field");

        public static readonly string InvalidRequest = GetErrorType("invalid-request");
        
        public static readonly string NotFound = GetErrorType("not-found");
        
        public static readonly string ValidationError = GetErrorType("validation-error");

        public static string GetErrorType(string error)
        {
            return $"{DefaultNamespace}/{error}";
        }
    }
}