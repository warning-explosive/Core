namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using UnitOfWork;

    internal class DataAccessOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, GenericEndpoint.DataAccess.UnitOfWork.IntegrationUnitOfWork>(EnLifestyle.Scoped);
            container.Override<IOutboxDelivery, GenericEndpoint.DataAccess.UnitOfWork.OutboxDelivery>(EnLifestyle.Scoped);
        }
    }
}