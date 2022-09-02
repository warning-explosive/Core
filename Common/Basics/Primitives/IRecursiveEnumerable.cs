namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// IRecursiveEnumerable
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IRecursiveEnumerable<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Moves source enumerator and gets next value
        /// </summary>
        /// <param name="item">Next item or default value</param>
        /// <returns>Next or default value</returns>
        bool TryMoveNext([NotNullWhen(true)] out T? item);
    }
}