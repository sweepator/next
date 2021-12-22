using shortid;
using shortid.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Extensions.Logging
{
    internal sealed class LogScopeContextAccessor
    {
        private const int DebugLogLevelThreshold = 3;

        private static AsyncLocal<LogScopeContextHolder> CurrentLogScopeContext = new AsyncLocal<LogScopeContextHolder>();

        private Stack<string> Context
        {
            get
            {
                return CurrentLogScopeContext.Value?.Context;
            }
            set
            {
                var holder = CurrentLogScopeContext.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    CurrentLogScopeContext.Value = new LogScopeContextHolder { Context = value };
                }
            }
        }

        private static string GenerateActionId()
        {
            string id = ShortId.Generate(new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 15
            });

            return id;
        }

        public string GetNext(out string spanId)
        {
            spanId = null;

            if (this.Context == null)
            {
                this.Context = new Stack<string>();
            }

            string id = GenerateActionId();
            this.Context.Push(id);
            spanId = string.Join('|', this.Context.Reverse());

            return id;
        }

        public string GetCurrent()
        {
            return this.Context?.LastOrDefault();
        }

        public int GetCurrentLevel()
        {
            return (int)this.Context?.Count();
        }

        public LogLevel GetDefaultLogLevel()
        {
            return GetCurrentLevel() < DebugLogLevelThreshold ? LogLevel.Information : LogLevel.Debug;
        }

        public void RemoveLast()
        {
            this.Context.Pop();
        }

        private class LogScopeContextHolder
        {
            public Stack<string> Context;
        }
    }
}