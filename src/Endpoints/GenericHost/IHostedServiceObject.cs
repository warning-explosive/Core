namespace SpaceEngineers.Core.GenericHost
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IHostedServiceObject
    /// </summary>
    public interface IHostedServiceObject
    {
        /// <summary>
        /// Runs host object
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Run(CancellationToken token);
    }
}