namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// IRetryStrategy abstraction
    /// </summary>
    public interface IRetryPolicy : IResolvable
    {
        /// <summary>
        /// Apply retry strategy
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing retry operation</returns>
        Task Apply(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token);
    }
}