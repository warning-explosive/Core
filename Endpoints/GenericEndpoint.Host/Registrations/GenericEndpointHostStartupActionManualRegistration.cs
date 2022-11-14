namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;
    using StartupActions;

    internal class GenericEndpointHostStartupActionManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<GenericEndpointHostStartupAction, GenericEndpointHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, GenericEndpointHostStartupAction>(EnLifestyle.Singleton);

            container.Register<MessagingHostStartupAction, MessagingHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, MessagingHostStartupAction>(EnLifestyle.Singleton);
        }
    }
}