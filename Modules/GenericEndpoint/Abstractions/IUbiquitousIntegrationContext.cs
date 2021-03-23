namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using Contract.Abstractions;

    /// <summary>
    /// Ubiquitous integration context
    /// Use to produce integration messages outside or within the endpoint scope
    /// </summary>
    public interface IUbiquitousIntegrationContext : IResolvable
    {
        /// <summary>
        /// Send integration command to logical owner
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Publish integration event to subscribers
        /// </summary>
        /// <param name="integrationEvent">Integration event</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>Ongoing publish operation</returns>
        Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent;
    }
}