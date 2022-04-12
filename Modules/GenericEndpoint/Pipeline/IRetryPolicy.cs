namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IRetryStrategy abstraction
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Apply retry strategy
        /// </summary>
        /// <param name="context">Integration context</param>
        /// <param name="exception">Processing error</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing retry operation</returns>
        Task Apply(IAdvancedIntegrationContext context, Exception exception, CancellationToken token);
    }
}