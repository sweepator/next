using System;

namespace Microsoft.Extensions.Logging
{

    public static class LoggerExtensions
    {
        public static void Error(this ILogger logger,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogError(messageTemplate, propertyValues);
        }

        public static void Error(this ILogger logger,
            Exception ex,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogError(ex, messageTemplate, propertyValues);
        }

        public static void Warning(this ILogger logger,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogWarning(messageTemplate, propertyValues);
        }

        public static void Warning(this ILogger logger,
            Exception ex,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogWarning(ex, messageTemplate, propertyValues);
        }

        public static void Debug(this ILogger logger,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogDebug(messageTemplate, propertyValues);
        }

        public static void Debug(this ILogger logger,
            Exception ex,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogDebug(ex, messageTemplate, propertyValues);
        }

        public static void Info(this ILogger logger,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogInformation(messageTemplate, propertyValues);
        }

        public static void Info(this ILogger logger,
            Exception ex,
            string messageTemplate,
            params object[] propertyValues)
        {
            logger.LogInformation(ex, messageTemplate, propertyValues);
        }
    }
}
