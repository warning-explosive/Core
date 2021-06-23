namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IRunnableEndpoint
    /// </summary>
    public interface IRunnableEndpoint : IResolvable
    {
        /// <summary>
        /// Starts endpoint instance
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing start operation</returns>
        Task StartAsync(CancellationToken token);

        /// <summary>
        /// Stops endpoint instance
        /// </summary>
        /// <returns>Ongoing stop operation</returns>
        Task StopAsync();
    }
}