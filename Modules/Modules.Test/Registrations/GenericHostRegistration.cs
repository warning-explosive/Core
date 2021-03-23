namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using GenericHost;

    internal class GenericHostRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            GenericHost
                .InMemoryIntegrationTransport(new InMemoryIntegrationTransportOptions())
                .Registration
                .Register(container);
        }
    }
}