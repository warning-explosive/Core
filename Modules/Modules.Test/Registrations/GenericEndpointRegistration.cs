namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using GenericEndpoint;
    using GenericHost;

    internal class GenericEndpointRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);

            GenericHost.GenericEndpointRegistration(endpointIdentity).Register(container);
        }
    }
}