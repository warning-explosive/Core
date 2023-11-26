namespace SpaceEngineers.Core.Test.WebApplication.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost;
    using StartupActions;

    internal class PurgeRabbitMqQueuesManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<PurgeRabbitMqQueuesHostedServiceStartupAction, PurgeRabbitMqQueuesHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, PurgeRabbitMqQueuesHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, PurgeRabbitMqQueuesHostedServiceStartupAction>(EnLifestyle.Singleton);
        }
    }
}