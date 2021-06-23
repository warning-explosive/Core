namespace SpaceEngineers.Core.Basics.Primitives
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Async counterpart for AutoResetEvent
    /// </summary>
    public class AsyncAutoResetEvent
    {
        private static readonly TaskCompletionSource<bool> CompletedSource
            = CreateCompletedCompletionSource(true);

        private readonly ConcurrentQueue<TaskCompletionSource<bool>> _waits
            = new ConcurrentQueue<TaskCompletionSource<bool>>();

        private int _completed;

        /// <summary> .cctor </summary>
        /// <param name="isSet">AsyncManualResetEvent initial state - signaled or not</param>
        public AsyncAutoResetEvent(bool isSet)
        {
            if (isSet)
            {
                Interlocked.Increment(ref _completed);
            }
        }

        /// <summary>
        /// Wait signaled state asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Waiting operation</returns>
        public Task WaitAsync(CancellationToken? cancellationToken = null)
        {
            if (_completed > 0)
            {
                Interlocked.Decrement(ref _completed);
                return CompletedSource.Task;
            }

            var tcs = CreateCompletionSource<bool>();
            _waits.Enqueue(tcs);

            return cancellationToken != null
                ? tcs.Task.WaitAsync(cancellationToken.Value)
                : tcs.Task;
        }

        /// <summary>
        /// Set event in signaled state atomically
        /// </summary>
        public void Set()
        {
            if (_waits.TryDequeue(out var toRelease))
            {
                toRelease.SetResult(true);
            }
            else
            {
                Interlocked.Increment(ref _completed);
            }
        }

        private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
        {
            return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private static TaskCompletionSource<TResult> CreateCompletedCompletionSource<TResult>(TResult result)
        {
            var tcs = CreateCompletionSource<TResult>();
            tcs.SetResult(result);
            return tcs;
        }
    }
}