namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IMessageHandlerMiddleware abstraction
    /// </summary>
    public interface IMessageHandlerMiddleware
    {
        /// <summary>
        /// Handle incoming message
        /// </summary>
        /// <param name="context">Integration context</param>
        /// <param name="next">Next message handler middleware</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing handle task</returns>
        [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
        Task Handle(
            IAdvancedIntegrationContext context,
            Func<IAdvancedIntegrationContext, CancellationToken, Task> next,
            CancellationToken token);
    }
}