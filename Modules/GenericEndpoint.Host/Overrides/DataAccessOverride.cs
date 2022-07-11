namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using UnitOfWork;
    using OutboxDelivery = DataAccess.UnitOfWork.OutboxDelivery;

    internal class DataAccessOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, GenericEndpoint.DataAccess.UnitOfWork.IntegrationUnitOfWork>(EnLifestyle.Scoped);
            container.Override<IOutboxDelivery, OutboxDelivery>(EnLifestyle.Scoped);
        }
    }
}