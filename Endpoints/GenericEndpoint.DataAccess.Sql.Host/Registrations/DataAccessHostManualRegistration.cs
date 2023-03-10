namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.Registrations
{
    using BackgroundWorkers;
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;
    using StartupActions;

    internal class DataAccessHostManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<InboxInvalidationHostStartupAction, InboxInvalidationHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, InboxInvalidationHostStartupAction>(EnLifestyle.Singleton);

            container.Register<GenericEndpointDataAccessHostBackgroundWorker, GenericEndpointDataAccessHostBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostBackgroundWorker, GenericEndpointDataAccessHostBackgroundWorker>(EnLifestyle.Singleton);
        }
    }
}