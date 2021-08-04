namespace SpaceEngineers.Core.Basics.Primitives
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// AsyncEnumerable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public class AsyncEnumerable<T> : IEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _source;
        private readonly CancellationToken _token;

        /// <summary> .cctor </summary>
        /// <param name="source">IAsyncEnumerable source</param>
        /// <param name="token">Cancellation token</param>
        public AsyncEnumerable(IAsyncEnumerable<T> source, CancellationToken token)
        {
            _source = source;
            _token = token;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumerator(_source, _token);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class AsyncEnumerator : IEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> _asyncEnumerator;
            private readonly CancellationToken _token;

            public AsyncEnumerator(IAsyncEnumerable<T> source, CancellationToken token)
            {
                _asyncEnumerator = source.GetAsyncEnumerator(token);
                _token = token;
            }

            public T Current => _asyncEnumerator.Current;

            object? IEnumerator.Current => _asyncEnumerator.Current;

            public bool MoveNext()
            {
                return _asyncEnumerator.MoveNextAsync().AsTask().Result;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
                _asyncEnumerator.DisposeAsync().AsTask().Wait(_token);
            }
        }
    }
}