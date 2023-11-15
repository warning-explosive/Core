namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using CrossCuttingConcerns.Settings;
    using Settings;

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
            container.Register(typeof(ISettingsProvider<>), typeof(GenericEndpointAppSettingsJsonSettingsProvider<>), EnLifestyle.Singleton);
        }
    }
}