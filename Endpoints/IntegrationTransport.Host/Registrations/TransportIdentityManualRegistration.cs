namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using Api;
    using CompositionRoot.Registration;

    internal class TransportIdentityManualRegistration : IManualRegistration
    {
        private readonly TransportIdentity _transportIdentity;

        public TransportIdentityManualRegistration(TransportIdentity transportIdentity)
        {
            _transportIdentity = transportIdentity;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_transportIdentity);
        }
    }
}