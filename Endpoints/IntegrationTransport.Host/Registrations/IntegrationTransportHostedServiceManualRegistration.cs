namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using BackgroundWorkers;
    using CompositionRoot.Registration;
    using GenericHost;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using StartupActions;

    internal class IntegrationTransportHostedServiceManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IntegrationTransportHostedServiceStartupAction, IntegrationTransportHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, IntegrationTransportHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, IntegrationTransportHostedServiceStartupAction>(EnLifestyle.Singleton);

            container.Register<IntegrationTransportHostedServiceBackgroundWorker, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceBackgroundWorker, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
        }
    }
}