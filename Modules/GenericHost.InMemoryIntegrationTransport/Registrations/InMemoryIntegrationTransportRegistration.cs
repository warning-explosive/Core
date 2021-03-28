namespace SpaceEngineers.Core.GenericHost.InMemoryIntegrationTransport.Registrations
{
    using AutoRegistration.Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    internal class InMemoryIntegrationTransportRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterCollection<IMessageHeaderProvider>(new[] { typeof(IntegratedMessageHeader) });
        }
    }
}