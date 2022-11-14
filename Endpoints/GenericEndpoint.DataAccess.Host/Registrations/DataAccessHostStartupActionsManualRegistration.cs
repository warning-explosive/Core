namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.Registrations
{
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;
    using StartupActions;

    internal class DataAccessHostStartupActionsManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<UpgradeDatabaseHostStartupAction, UpgradeDatabaseHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, UpgradeDatabaseHostStartupAction>(EnLifestyle.Singleton);

            container.Register<EventSourcingHostStartupAction, EventSourcingHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, EventSourcingHostStartupAction>(EnLifestyle.Singleton);

            container.Register<InboxInvalidationHostStartupAction, InboxInvalidationHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, InboxInvalidationHostStartupAction>(EnLifestyle.Singleton);
        }
    }
}