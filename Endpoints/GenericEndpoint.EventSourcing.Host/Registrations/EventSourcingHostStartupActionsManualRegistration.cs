namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;
    using StartupActions;

    internal class EventSourcingHostStartupActionsManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Register<EventSourcingHostStartupAction, EventSourcingHostStartupAction>(EnLifestyle.Singleton);
            container.Advanced.RegisterCollectionEntry<IHostStartupAction, EventSourcingHostStartupAction>(EnLifestyle.Singleton);
        }
    }
}