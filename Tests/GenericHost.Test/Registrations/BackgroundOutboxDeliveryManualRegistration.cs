namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using CompositionRoot.Registration;
    using GenericEndpoint.UnitOfWork;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class BackgroundOutboxDeliveryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterDecorator<IOutboxDelivery, BackgroundOutboxDelivery>(EnLifestyle.Scoped);
        }
    }
}