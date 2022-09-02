namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IAsyncQueue abstraction
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IAsyncQueue<TElement>
    {
        /// <summary> Enqueue </summary>
        /// <param name="element">Element</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Enqueue(TElement element, CancellationToken token);

        /// <summary>
        /// Starts elements processing
        /// </summary>
        /// <param name="callback">Callback function for next element</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Run(Func<TElement, CancellationToken, Task> callback, CancellationToken token);
    }
}