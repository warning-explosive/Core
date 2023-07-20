namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Postgres.Host.Registrations;

using CompositionRoot.Registration;
using GenericHost;
using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
using SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.BackgroundWorkers;
using StartupActions;

internal class DataAccessHostManualRegistration : IManualRegistration
{
    public void Register(IManualRegistrationsContainer container)
    {
        container.Register<InboxInvalidationHostedServiceStartupAction, InboxInvalidationHostedServiceStartupAction>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, InboxInvalidationHostedServiceStartupAction>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceObject, InboxInvalidationHostedServiceStartupAction>(EnLifestyle.Singleton);

        container.Register<ReloadNpgsqlTypesHostedServiceStartupAction, ReloadNpgsqlTypesHostedServiceStartupAction>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, ReloadNpgsqlTypesHostedServiceStartupAction>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceObject, ReloadNpgsqlTypesHostedServiceStartupAction>(EnLifestyle.Singleton);

        container.Register<GenericEndpointDataAccessHostedServiceBackgroundWorker, GenericEndpointDataAccessHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceBackgroundWorker, GenericEndpointDataAccessHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
        container.Advanced.RegisterCollectionEntry<IHostedServiceObject, GenericEndpointDataAccessHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
    }
}