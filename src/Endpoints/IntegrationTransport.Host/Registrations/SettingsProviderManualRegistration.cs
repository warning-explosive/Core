namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using Settings;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class SettingsProviderManualRegistration : IManualRegistration
    {
        private readonly SettingsDirectoryProvider _settingsDirectoryProvider;

        public SettingsProviderManualRegistration(SettingsDirectoryProvider settingsDirectoryProvider)
        {
            _settingsDirectoryProvider = settingsDirectoryProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_settingsDirectoryProvider);
            container.Register(typeof(ISettingsProvider<>), typeof(IntegrationTransportAppSettingsJsonSettingsProvider<>), EnLifestyle.Singleton);
        }
    }
}