namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;
    using GenericEndpoint.Abstractions;

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
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Ongoing retry operation</returns>
        Task Apply<TMessage>(TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage;
    }
}