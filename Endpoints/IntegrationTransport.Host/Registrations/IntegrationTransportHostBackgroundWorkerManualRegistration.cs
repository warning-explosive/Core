namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using BackgroundWorkers;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;

    internal class IntegrationTransportHostBackgroundWorkerManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<IntegrationTransportHostBackgroundWorker, IntegrationTransportHostBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostBackgroundWorker, IntegrationTransportHostBackgroundWorker>(EnLifestyle.Singleton);
        }
    }
}