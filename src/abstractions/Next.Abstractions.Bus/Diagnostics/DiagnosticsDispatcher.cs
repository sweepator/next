using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Diagnostics
{
    internal class DiagnosticsDispatcher : IMessageDispatcher
    {
        private readonly IMessageDispatcher _innerDispatcher;
        private readonly ILogger<DiagnosticsDispatcher> _logger;
        private readonly ConcurrentDictionary<string, Counter> _metrics;

        public DiagnosticsDispatcher(
            IMessageDispatcher innerDispatcher,
            ILogger<DiagnosticsDispatcher> logger)
        {
            _innerDispatcher = innerDispatcher;
            _logger = logger;
            _metrics = new ConcurrentDictionary<string, Counter>();
        }

        public async Task<bool> ProcessMessage(TransportMessage message)
        {
            var sw = Stopwatch.StartNew();
            var handled = await _innerDispatcher.ProcessMessage(message);
            sw.Stop();

            var key = $"{message.Name}:{message.Headers[MessageHeaders.Component]}";
            
            var counter = _metrics.GetOrAdd(key, k => new Counter());
            Interlocked.Increment(ref counter.NMessages);
            Interlocked.Add(ref counter.TotalMS, sw.ElapsedMilliseconds);

            _logger.LogDebug($"Processed {key} in {sw.ElapsedMilliseconds}ms with avg {counter.GetAvg().TotalMilliseconds}ms");

            return handled;
        }
        
        class Counter
        {
            public long NMessages;
            public long TotalMS;

            public TimeSpan GetAvg()
            {
                var time = Interlocked.Read(ref TotalMS) / Interlocked.Read(ref NMessages);
                return TimeSpan.FromMilliseconds(time);
            }

            public override string ToString()
            {
                return GetAvg().ToString();
            }
        }
    }
}