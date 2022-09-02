namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ.Settings
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;

    [Component(EnLifestyle.Singleton)]
    internal class RabbitMqSettingsProvider : BaseSettingsProvider<RabbitMqSettings>,
                                              IResolvable<ISettingsProvider<RabbitMqSettings>>
    {
        public RabbitMqSettingsProvider(ISettingsProvider<RabbitMqSettings> underlyingSettingProvider)
            : base(underlyingSettingProvider)
        {
        }
    }
}