namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Settings;
    using Settings;

    internal class SettingsProviderOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<ISettingsScopeProvider, EndpointSettingsScopeProvider>(EnLifestyle.Singleton);
        }
    }
}