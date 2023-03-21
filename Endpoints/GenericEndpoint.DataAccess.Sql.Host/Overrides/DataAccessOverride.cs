namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.Overrides
{
    using CompositionRoot.Registration;
    using GenericEndpoint.UnitOfWork;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using UnitOfWork;

    internal class DataAccessOverride : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, IntegrationUnitOfWork>(EnLifestyle.Scoped);
            container.Override<IOutboxDelivery, OutboxDelivery>(EnLifestyle.Singleton);
        }
    }
}