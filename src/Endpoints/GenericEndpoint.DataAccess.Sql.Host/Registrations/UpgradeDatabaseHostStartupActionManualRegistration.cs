namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.Registrations
{
    using CompositionRoot.Registration;
    using GenericHost;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using StartupActions;

    internal class UpgradeDatabaseHostStartupActionManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<UpgradeDatabaseHostedServiceStartupAction, UpgradeDatabaseHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, UpgradeDatabaseHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, UpgradeDatabaseHostedServiceStartupAction>(EnLifestyle.Singleton);
        }
    }
}