namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Api.Abstractions;
    using CrossCuttingConcerns.Settings;
    using Settings;

    internal class SettingsProviderOverrides : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<ISettingsScopeProvider, DefaultSettingsScopeProvider, EndpointSettingsScopeProvider>(EnLifestyle.Singleton);
        }
    }
}