namespace SpaceEngineers.Core.GenericHost.Test.Overrides
{
    using GenericEndpoint.UnitOfWork;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class OutboxMessagesDeliveryOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IOutboxDelivery, BackgroundOutboxDelivery>(EnLifestyle.Scoped);
        }
    }
}