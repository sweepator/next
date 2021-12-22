using Next.Abstractions.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Logging
{
    public static class LogScopeExtensions
    {
        private static readonly LogScopeContextAccessor LogScopeContextAccessor = new LogScopeContextAccessor();

        private static Dictionary<string, object> GetState(
            string scopeName,
            string logCategory = null,
            params ValueTuple<string, object>[] properties)
        {
            var parentId = LogScopeContextAccessor.GetCurrent();

            var state = properties != null ?
                        properties.ToDictionary(p => p.Item1, p => p.Item2) :
                        new Dictionary<string, object>();

            state.Add(nameof(Properties.ScopeId), LogScopeContextAccessor.GetNext(out var spanId));
            state.Add(nameof(Properties.ScopeName), scopeName);

            if (!string.IsNullOrEmpty(spanId))
            {
                state.Add(nameof(Properties.SpanScopeId), spanId);
            }

            if (!string.IsNullOrEmpty(logCategory))
            {
                state.Add(nameof(Properties.LogCategory), logCategory);
            }

            return state;
        }

        private static void OnOperationDisposed(object sender, EventArgs e)
        {
            var operation = (LogScope)sender;

            LogScopeContextAccessor.RemoveLast();
            operation.Disposed -= OnOperationDisposed;
        }

        public static LogScope NewScope(this ILogger logger,
            string scopeName,
            LogLevel? logLevel = null,
            string logCategory = null,
            params ValueTuple<string, object>[] properties)
        {
            var operation = new LogScope(
                logger,
                scopeName,
                GetState(
                    scopeName,
                    logCategory,
                    properties),
                CompletionBehaviour.Abandon,
                logLevel.HasValue ? logLevel.Value : LogScopeContextAccessor.GetDefaultLogLevel());

            operation.Disposed += OnOperationDisposed;
            return operation;
        }

        public static IDisposable ScopeAt(this ILogger logger,
           string scopeName,
           LogLevel? logLevel = null,
           string logCategory = null,
           params ValueTuple<string, object>[] properties)
        {
            var operation = new LogScope(
               logger,
               scopeName,
               GetState(
                   scopeName,
                   logCategory,
                   properties),
               CompletionBehaviour.Complete,
               logLevel.HasValue ? logLevel.Value : LogScopeContextAccessor.GetDefaultLogLevel());

            operation.Disposed += OnOperationDisposed;
            return operation;
        }
    }
}
