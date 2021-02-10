namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Integration context
    /// Used for managing integration operations between endpoints
    /// </summary>
    public interface IIntegrationContext
    {
        /// <summary>
        /// Send integration command to logical owner
        /// </summary>
        /// <param name="integrationCommand">Integration command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Send<TCommand>(TCommand integrationCommand, CancellationToken cancellationToken)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Publish integration event to subscribers
        /// </summary>
        /// <param name="integrationEvent">Integration event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>Ongoing publish operation</returns>
        Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
            where TEvent : IIntegrationEvent;
    }
}