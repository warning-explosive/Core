namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.Overrides
{
    using CompositionRoot.Registration;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericEndpoint.UnitOfWork;

    internal class DataAccessOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, UnitOfWork.IntegrationUnitOfWork>(EnLifestyle.Scoped);
            container.Override<IOutboxDelivery, UnitOfWork.OutboxDelivery>(EnLifestyle.Singleton);
        }
    }
}