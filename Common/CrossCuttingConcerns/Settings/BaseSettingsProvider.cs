namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// BaseSettingsProvider
    /// </summary>
    /// <typeparam name="TSettings">TSettings type-argument</typeparam>
    public abstract class BaseSettingsProvider<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class, ISettings, new()
    {
        private readonly ISettingsProvider<TSettings> _underlyingSettingProvider;

        /// <summary> .cctor </summary>
        /// <param name="underlyingSettingProvider">Underlying ISettingsProvider</param>
        protected BaseSettingsProvider(ISettingsProvider<TSettings> underlyingSettingProvider)
        {
            _underlyingSettingProvider = underlyingSettingProvider;
        }

        /// <inheritdoc />
        public Task<TSettings> Get(CancellationToken token)
        {
            return _underlyingSettingProvider.Get(token);
        }
    }
}