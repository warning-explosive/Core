namespace SpaceEngineers.Core.IntegrationTransport.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Pipeline;
    using Integration;

    internal class IntegrationTransportOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationTypeProvider, IntegrationTransportIntegrationTypeProvider>(EnLifestyle.Singleton);
            container.Override<IMessagesCollector, IntegrationTransportMessagesCollector>(EnLifestyle.Scoped);
        }
    }
}