namespace SpaceEngineers.Core.CrossCuttingConcerns.Settings
{
    using CompositionRoot.Registration;

    /// <summary>
    /// SettingsDirectoryProviderManualRegistration
    /// </summary>
    public class SettingsDirectoryProviderManualRegistration : IManualRegistration
    {
        private readonly SettingsDirectoryProvider _settingsDirectoryProvider;

        /// <summary> .cctor </summary>
        /// <param name="settingsDirectoryProvider">SettingsDirectoryProvider</param>
        public SettingsDirectoryProviderManualRegistration(SettingsDirectoryProvider settingsDirectoryProvider)
        {
            _settingsDirectoryProvider = settingsDirectoryProvider;
        }

        /// <inheritdoc />
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_settingsDirectoryProvider);
        }
    }
}