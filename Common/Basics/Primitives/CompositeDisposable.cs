namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// CompositeDisposable
    /// </summary>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct CompositeDisposable : IDisposable
    {
        private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

        internal CompositeDisposable(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
                _disposables.Push(disposable);
            }
        }

        /// <summary> Push </summary>
        /// <param name="disposable">IDisposable</param>
        public void Push(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _disposables.Each(d => d.Dispose());
            _disposables.Clear();
        }
    }
}