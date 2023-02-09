namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// EmptyDisposable
    /// </summary>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct EmptyDisposable : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}