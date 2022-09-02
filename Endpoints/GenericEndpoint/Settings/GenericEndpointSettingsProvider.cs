namespace SpaceEngineers.Core.GenericEndpoint.Settings
{
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointSettingsProvider : BaseSettingsProvider<GenericEndpointSettings>,
                                                     IResolvable<ISettingsProvider<GenericEndpointSettings>>
    {
        public GenericEndpointSettingsProvider(ISettingsProvider<GenericEndpointSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}