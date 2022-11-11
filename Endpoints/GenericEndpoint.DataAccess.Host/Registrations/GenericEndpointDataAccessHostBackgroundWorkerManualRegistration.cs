namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.Registrations
{
    using BackgroundWorkers;
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    internal class GenericEndpointDataAccessHostBackgroundWorkerManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<GenericEndpointDataAccessHostBackgroundWorker, GenericEndpointDataAccessHostBackgroundWorker>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostBackgroundWorker, GenericEndpointDataAccessHostBackgroundWorker>(EnLifestyle.Singleton);
        }
    }
}