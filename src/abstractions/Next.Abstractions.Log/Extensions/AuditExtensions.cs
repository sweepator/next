using Next.Abstractions.Log;
using shortid;
using shortid.Configuration;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging
{
    public static class AuditExtensions
    {
        private static string GenerateAuditId()
        {
            string id = ShortId.Generate(new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 15
            });

            return id;
        }

        public static void LogAudit(this ILogger logger,
            string userId,
            string actionName,
            object request,
            object response,
            LogLevel logLevel = LogLevel.Information)
        {
            using (logger.ScopeAt(
                    actionName,
                    logLevel,
                    LogCategory.Audit,
                    ("UserId", userId),
                    ("Request", request),
                    ("Response", response)))
            {
                logger.Log(logLevel, "{actionName} executed by {userId} with {@request} -> {@response}",
                    actionName,
                    userId,
                    request,
                    response);
            }
        }
    }
}
