namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IRetryStrategy abstraction
    /// </summary>
    public interface IRetryPolicy : IResolvable
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