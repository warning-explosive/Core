namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;
    using StartupActions;

    internal class UpgradeDatabaseHostStartupActionManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<UpgradeDatabaseHostStartupAction, UpgradeDatabaseHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, UpgradeDatabaseHostStartupAction>(EnLifestyle.Singleton);
        }
    }
}