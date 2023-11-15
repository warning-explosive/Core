namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Primitives;

    /// <summary>
    /// Exclusive
    /// </summary>
    public class Exclusive
    {
        private readonly AsyncAutoResetEvent _sync;

        private bool _isTaken;

        /// <summary> .cctor </summary>
        public Exclusive()
        {
            _sync = new AsyncAutoResetEvent(true);
            _isTaken = false;
        }

        /// <summary>
        /// Ensures
        /// </summary>
        /// <param name="token">CancellationToken</param>
        /// <returns>Ongoing operation</returns>
        public async Task<IDisposable> Run(CancellationToken token)
        {
            if (_isTaken)
            {
                throw new InvalidOperationException("Exclusive operation has already been started");
            }

            await _sync
               .WaitAsync(token)
               .ConfigureAwait(false);

            _isTaken = true;

            return Disposable.Create(this, Finally);

            static void Finally(Exclusive exclusive)
            {
                exclusive._sync.Set();
                exclusive._isTaken = false;
            }
        }
    }
}