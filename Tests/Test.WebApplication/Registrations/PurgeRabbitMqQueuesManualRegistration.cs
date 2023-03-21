namespace SpaceEngineers.Core.Test.WebApplication.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;
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