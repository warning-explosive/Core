namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IRunnableEndpoint
    /// </summary>
    public interface IRunnableEndpoint
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
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing stop operation</returns>
        Task StopAsync(CancellationToken token);
    }
}