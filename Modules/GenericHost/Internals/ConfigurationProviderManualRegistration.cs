namespace SpaceEngineers.Core.GenericHost.Internals
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using CrossCuttingConcerns.Settings;

    internal class ConfigurationProviderManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IConfigurationProvider, ConfigurationProvider>(EnLifestyle.Singleton);
        }
    }
}