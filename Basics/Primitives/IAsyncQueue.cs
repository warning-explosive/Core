namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IAsyncQueue abstraction
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IAsyncQueue<out TElement>
    {
        /// <summary>
        /// Starts message processing
        /// </summary>
        /// <param name="callback">Callback function for future message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing scheduler operation</returns>
        public Task Run(Func<TElement, Task> callback, CancellationToken token);
    }
}