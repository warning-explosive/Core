namespace SpaceEngineers.Core.GenericHost.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IHostBackgroundWorker
    /// </summary>
    public interface IHostBackgroundWorker
    {
        /// <summary>
        /// Runs host background worker
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing background worker</returns>
        Task Run(CancellationToken token);
    }
}