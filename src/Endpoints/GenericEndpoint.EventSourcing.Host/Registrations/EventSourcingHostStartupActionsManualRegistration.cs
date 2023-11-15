namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost;
    using StartupActions;

    internal class EventSourcingHostStartupActionsManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<EventSourcingHostedServiceStartupAction, EventSourcingHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceStartupAction, EventSourcingHostedServiceStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostedServiceObject, EventSourcingHostedServiceStartupAction>(EnLifestyle.Singleton);
        }
    }
}