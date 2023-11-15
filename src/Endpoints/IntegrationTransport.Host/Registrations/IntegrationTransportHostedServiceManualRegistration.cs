namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using BackgroundWorkers;
    using CompositionRoot.Registration;
    using GenericHost;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class IntegrationTransportHostedServiceManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterEmptyCollection<IHostedServiceStartupAction>();

            container.Register<IntegrationTransportHostedServiceBackgroundWorker, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceBackgroundWorker, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, IntegrationTransportHostedServiceBackgroundWorker>(EnLifestyle.Singleton);
        }
    }
}