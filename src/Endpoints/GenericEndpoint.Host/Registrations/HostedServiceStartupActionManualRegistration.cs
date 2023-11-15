namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost;
    using StartupActions;

    internal class HostedServiceStartupActionManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<GenericEndpointHostedServiceStartupAction, GenericEndpointHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, GenericEndpointHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, GenericEndpointHostedServiceStartupAction>(EnLifestyle.Singleton);

            container.Register<MessagingHostedServiceStartupAction, MessagingHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, MessagingHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, MessagingHostedServiceStartupAction>(EnLifestyle.Singleton);
        }
    }
}