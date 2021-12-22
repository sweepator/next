using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Handles all the logic to receive messages from an IInboundTransport in a message loop.
    /// Handles success and failure scenarios.
    /// Actual message processing is delegated to the IMessageDispatcher.
    /// </summary>
    public class MessageWorker : IDisposable
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IInboundTransport _receiver;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancellation;
        private readonly int _concurrencyLevel;

        private readonly ManualResetEventSlim _workWaitHandle;
        private long _workCount;

        private readonly Task _workerTask;
        private readonly ManualResetEventSlim _runningWaitHandle;
        
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan SleepTimeout = TimeSpan.FromSeconds(1);

        public MessageWorker(
            IInboundTransport receiver,
            IMessageDispatcher messageDispatcher,
            ILoggerFactory loggerFactory,
            int concurrencyLevel = 200)
        {
            _receiver = receiver;
            _messageDispatcher = messageDispatcher;
            _logger = loggerFactory.CreateLogger(typeof(MessageWorker));
            _cancellation = new CancellationTokenSource();
            _concurrencyLevel = concurrencyLevel;

            _workWaitHandle = new ManualResetEventSlim(true);
            _workCount = 0;

            _runningWaitHandle = new ManualResetEventSlim(false);
            _workerTask = Task.Factory.StartNew(
                async () => await Work(), 
                TaskCreationOptions.LongRunning);
        }

        public void Start()
        {
            _runningWaitHandle.Set();
        }

        public void Stop()
        {
            _runningWaitHandle.Reset();

            if (_receiver is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public bool IsRunning => _runningWaitHandle.IsSet;

        private async Task Work()
        {
            // message loop
            while (!_cancellation.IsCancellationRequested)
            {
                if (!IsRunning)
                {
                    // wait 1 second before checking again
                    WaitForRunSignal();
                    continue;
                }

                if (!HasSlots())
                {
                    WaitForSlots();
                }
                else
                {
                    var transaction = await GetMessage();
                    if (transaction != null)
                    {
                        AcquireSlot();

                        await Task.Run(() => ProcessTransaction(transaction))
                            .ContinueWith(t => ReleaseSlot());
                    }
                }
            }
        }

        private void WaitForSlots()
        {
            _workWaitHandle.Wait(SleepTimeout);
        }

        private bool HasSlots()
        {
            return _workWaitHandle.IsSet && Interlocked.Read(ref _workCount) < _concurrencyLevel;
        }

        private void WaitForRunSignal()
        {
            _runningWaitHandle.Wait(SleepTimeout);
        }

        private async Task<IMessageTransaction> GetMessage()
        {
            IMessageTransaction transaction = null;

            try
            {
                transaction = await _receiver.Receive(ReceiveTimeout);
            }
            catch (Exception ex)
            {
                // log error
                _logger.LogError(ex, "Failed to receive message");
                await Task.Delay(5000);
            }

            return transaction;
        }

        private void AcquireSlot()
        {
            if (Interlocked.Increment(ref _workCount) == _concurrencyLevel)
            {
                if (_workWaitHandle.IsSet)
                {
                    _workWaitHandle.Reset();
                }
            }
        }

        private void ReleaseSlot()
        {
            Interlocked.Decrement(ref _workCount);

            if (!_workWaitHandle.IsSet)
            {
                _workWaitHandle.Set();
            }
        }

        private async Task ProcessTransaction(IMessageTransaction transaction)
        {
            Func<Task> completion = null;

            try
            {
                _logger.LogTrace("Processing message ({DeliveryCount}) {MessageName} with id {MessageId}: {Payload} {Headers}",
                    transaction.DeliveryCount,
                    transaction.Message.Name,
                    transaction.Message.Id,
                    transaction.Message.PayLoad,
                    transaction.Message.Headers);

                // process message within a logical transaction that can be used for processing idempotency
                var handled = await _messageDispatcher.ProcessMessage(transaction.Message);
                if (handled)
                {
                    completion = transaction.Commit;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process message {MessageName} with id {MessageId}",
                    transaction.Message.Name,
                    transaction.Message.Id);

                completion = transaction.Fail;
            }

            if (completion != null)
            {
                // try to complete the transaction, regardless of failures
                try
                {
                    await completion();
                    
                    _logger.LogTrace(
                        "Processed message ({DeliveryCount}) {MessageName} successfully with id {MessageId}: {Payload} {Headers}",
                        transaction.DeliveryCount,
                        transaction.Message.Name,
                        transaction.Message.Id,
                        transaction.Message.PayLoad,
                        transaction.Message.Headers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to complete message {MessageName} with id {MessageId}",
                        transaction.Message.Name,
                        transaction.Message.Id);
                }
            }
        }

        public void Dispose()
        {
            if (!_cancellation.IsCancellationRequested)
            {
                if (IsRunning)
                {
                    Stop();
                }

                _cancellation.Cancel();
                _workerTask.Wait();
            }
        }
    }
}