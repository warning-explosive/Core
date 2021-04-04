namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Mocks;

    internal class GenericEndpointTestRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);
            container.RegisterInstance(endpointIdentity);

            container.Register<IAdvancedIntegrationContext, AdvancedIntegrationContextMock>();
            container.Register<AdvancedIntegrationContextMock, AdvancedIntegrationContextMock>();
        }
    }
}