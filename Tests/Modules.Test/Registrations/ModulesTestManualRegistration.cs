namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using CompositionRoot.Api.Abstractions;
    using InMemoryIntegrationTransport.Host.Internals;

    internal class ModulesTestManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            new ModulesTestGenericEndpointIdentityManualRegistration().Register(container);
            new InMemoryIntegrationTransportManualRegistration().Register(container);
            new ModulesTestLoggerFactoryManualRegistration().Register(container);
            new ExtendedTypeProviderManualRegistration(Array.Empty<Type>()).Register(container);
            new ManuallyRegisteredServiceManualRegistration().Register(container);
        }
    }
}