namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.Overrides
{
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Registration;

    internal class DataAccessOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<SpaceEngineers.Core.GenericEndpoint.UnitOfWork.IIntegrationUnitOfWork, SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork.IntegrationUnitOfWork>(EnLifestyle.Scoped);
            container.Override<SpaceEngineers.Core.GenericEndpoint.UnitOfWork.IOutboxDelivery, SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.UnitOfWork.OutboxDelivery>(EnLifestyle.Singleton);
        }
    }
}