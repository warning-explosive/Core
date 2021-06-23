namespace SpaceEngineers.Core.GenericHost.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IHostStartupAction
    /// </summary>
    public interface IHostStartupAction
    {
        /// <summary>
        /// Runs host startup action
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing startup action</returns>
        Task Run(CancellationToken token);
    }
}