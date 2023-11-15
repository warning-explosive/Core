namespace SpaceEngineers.Core.Basics.Primitives
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Async counterpart for AsyncManualResetEvent
    /// </summary>
    public class AsyncManualResetEvent
    {
        private readonly object _sync;

        private TaskCompletionSource<bool> _tcs;

        /// <summary> .cctor </summary>
        /// <param name="isSet">AsyncManualResetEvent initial state - signaled or not</param>
        public AsyncManualResetEvent(bool isSet)
        {
            _sync = new object();

            _tcs = CreateCompletionSource<bool>();

            if (isSet)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Wait signaled state asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Waiting operation</returns>
        public Task WaitAsync(CancellationToken? cancellationToken = null)
        {
            Task waitTask;

            lock (_sync)
            {
                waitTask = _tcs.Task;
            }

            return waitTask.IsCompleted
                   || cancellationToken == null
                ? waitTask
                : waitTask.WaitAsync(cancellationToken.Value);
        }

        /// <summary>
        /// Set event in signaled state atomically
        /// </summary>
        public void Set()
        {
            lock (_sync)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Set event in non signaled state atomically
        /// Do nothing in case of non signaled state
        /// </summary>
        public void Reset()
        {
            lock (_sync)
            {
                if (_tcs.Task.IsCompleted)
                {
                    _tcs = CreateCompletionSource<bool>();
                }
            }
        }

        private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
        {
            return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}