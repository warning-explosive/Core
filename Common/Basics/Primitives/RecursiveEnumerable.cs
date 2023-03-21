namespace SpaceEngineers.Core.Basics.Primitives
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Recursive enumerable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public class RecursiveEnumerable<T> : IRecursiveEnumerable<T>
        where T : class
    {
        private readonly IEnumerator<T> _enumerator;

        /// <summary> .cctor </summary>
        /// <param name="enumerator">Source enumerator</param>
        public RecursiveEnumerable(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        /// <inheritdoc />
        public bool TryMoveNext([NotNullWhen(true)] out T? item)
        {
            if (_enumerator.MoveNext())
            {
                item = _enumerator.Current;
                return true;
            }

            item = default;
            return false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}