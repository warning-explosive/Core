namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IErrorHandler
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Executes error handler
        /// </summary>
        /// <param name="context">Integration context</param>
        /// <param name="exception">Processing error</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Handle(
            IAdvancedIntegrationContext context,
            Exception exception,
            CancellationToken token);
    }
}