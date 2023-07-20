namespace SpaceEngineers.Core.GenericHost.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericEndpoint.UnitOfWork;
    using Mocks;

    internal class BackgroundOutboxDeliveryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterDecorator<IOutboxDelivery, BackgroundOutboxDelivery>(EnLifestyle.Singleton);
        }
    }
}