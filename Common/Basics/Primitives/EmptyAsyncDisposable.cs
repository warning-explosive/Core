namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// EmptyAsyncDisposable
    /// </summary>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct EmptyAsyncDisposable : IAsyncDisposable
    {
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}