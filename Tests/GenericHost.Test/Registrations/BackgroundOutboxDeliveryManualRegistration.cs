namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using GenericEndpoint.UnitOfWork;
    using Mocks;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;

    internal class BackgroundOutboxDeliveryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterDecorator<IOutboxDelivery, BackgroundOutboxDelivery>(EnLifestyle.Scoped);
        }
    }
}