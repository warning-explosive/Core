namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.Registrations
{
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;
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