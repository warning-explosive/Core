namespace SpaceEngineers.Core.Modules.Test.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.DataAccess.UnitOfWork;
    using Mocks;

    internal class OutboxMessagesDeliveryOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IOutboxMessagesDelivery, BackgroundOutboxMessagesDelivery>(EnLifestyle.Scoped);
        }
    }
}