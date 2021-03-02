namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using GenericHost;

    internal class GenericHostRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            var options = new InMemoryIntegrationTransportOptions();
            GenericHost.InMemoryIntegrationTransport(options).Registration.Register(container);
        }
    }
}