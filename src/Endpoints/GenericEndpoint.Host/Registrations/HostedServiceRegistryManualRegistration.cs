namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using GenericHost;

    internal class HostedServiceRegistryManualRegistration : IManualRegistration
    {
        private readonly IHostedServiceRegistry _hostedServiceRegistry;

        public HostedServiceRegistryManualRegistration(IHostedServiceRegistry hostedServiceRegistry)
        {
            _hostedServiceRegistry = hostedServiceRegistry;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterInstance(_hostedServiceRegistry);
        }
    }
}