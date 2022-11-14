namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using Api.Abstractions;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using StartupActions;

    internal class PurgeRabbitMqQueuesManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<PurgeRabbitMqQueuesHostStartupAction, PurgeRabbitMqQueuesHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, PurgeRabbitMqQueuesHostStartupAction>(EnLifestyle.Singleton);
        }
    }
}