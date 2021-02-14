namespace SpaceEngineers.Core.Basics.Async
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Async counterpart for CountdownEvent
    /// Free interpretation with several differences from original sync event
    ///     Differences:
    ///     - Signaled state is state when inner counter has reached zero or initialized with zero
    ///     - Increment resets event to non signaled state
    ///     - Decrement sets signaled state if inner counter has reached zero value
    /// </summary>
    public class AsyncCountdownEvent
    {
        private readonly object _sync;
        private TaskCompletionSource<bool> _tcs;
        private int _count;

        /// <summary> .cctor </summary>
        /// <param name="count">Initial count</param>
        public AsyncCountdownEvent(int count)
        {
            _sync = new object();
            _tcs = CreateCompletionSource<bool>();
            _count = count;

            if (_count <= 0)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Wait signaled state asynchronously
        /// Signaled state is state when inner counter has reached zero
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
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
        /// Increments inner counter atomically and resets to non signaled state
        /// </summary>
        /// <returns>Actual counter state</returns>
        public int Increment()
        {
            lock (_sync)
            {
                if (_tcs.Task.IsCompleted)
                {
                    _tcs = CreateCompletionSource<bool>();
                }

                return ++_count;
            }
        }

        /// <summary>
        /// Decrements inner counter atomically
        /// Set signaled state if inner counter has reached zero value
        /// </summary>
        /// <returns>Actual counter state</returns>
        public int Decrement()
        {
            lock (_sync)
            {
                var result = --_count;

                if (result <= 0)
                {
                    _tcs.SetResult(true);
                }

                return result;
            }
        }

        /// <summary>
        /// Reads current counter state
        /// </summary>
        /// <returns>Actual counter state</returns>
        public int Read()
        {
            lock (_sync)
            {
                return _count;
            }
        }

        private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
        {
            return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}