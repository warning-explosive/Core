namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// Settings provider
    /// </summary>
    /// <typeparam name="TSettings">ISettings</typeparam>
    [SuppressMessage("Analysis", "CA1716", Justification = "Reviewed")]
    public interface ISettingsProvider<TSettings> : IResolvable
        where TSettings : class, ISettings
    {
        /// <summary>
        /// Get ISettings value
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Get operation with ISettings value as result</returns>
        Task<TSettings> Get(CancellationToken token);

        /// <summary>
        /// Set ISettings value
        /// </summary>
        /// <param name="value">ISettings value</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Set operation</returns>
        Task Set(TSettings value, CancellationToken token);
    }
}