using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Next.Abstractions.Log
{
    public sealed class LogScope : IDisposable
    {
        public event EventHandler Disposed;
        private const string OutcomeCompleted = "completed", OutcomeAbandoned = "abandoned";

        private readonly ILogger _logger;
        private readonly string _messageTemplate;
        private readonly Dictionary<string, object> _state;
        private readonly Stopwatch _stopwatch;
        private IDisposable _popContext;
        private readonly LogLevel _logLevel;
        private CompletionBehaviour _completionBehaviour;

        internal LogScope(
            ILogger logger, 
            string messageTemplate,
            Dictionary<string, object> state,
            CompletionBehaviour completionBehaviour,
            LogLevel logLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
            _state = state;
            _logLevel = logLevel;
            _completionBehaviour = completionBehaviour;

            _popContext = logger.BeginScope(state);
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            switch (_completionBehaviour)
            {
                case CompletionBehaviour.Silent:
                    break;

                case CompletionBehaviour.Abandon:
                    Write(_logger, _logLevel, OutcomeAbandoned);
                    break;

                case CompletionBehaviour.Complete:
                    Write(_logger, _logLevel, OutcomeCompleted);
                    break;

                default:
                    throw new InvalidOperationException("Unknown underlying state value");
            }

            PopLogContext();
        }

        public void Complete()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            _stopwatch.Stop();
            _completionBehaviour = CompletionBehaviour.Complete;
        }

        public void Abandon()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            _stopwatch.Stop();
            _completionBehaviour = CompletionBehaviour.Abandon;;
        }

        public void Cancel()
        {
            _stopwatch.Stop();
            _completionBehaviour = CompletionBehaviour.Silent;

            PopLogContext();
        }

        private void PopLogContext()
        {
            _popContext?.Dispose();
            _popContext = null;
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        private void Write(ILogger logger, LogLevel level, string outcome)
        {
            var elapsed = _stopwatch.Elapsed.TotalMilliseconds;

            using (logger.BeginScope(new Dictionary<string, object>()
            {
                { nameof(Properties.Elapsed), elapsed },
                { nameof(Properties.Outcome), outcome }
            }))
            {

                logger.Log(level, $"{_messageTemplate} {outcome} in {elapsed} ms");
            }

            PopLogContext();
        }
    }
}
