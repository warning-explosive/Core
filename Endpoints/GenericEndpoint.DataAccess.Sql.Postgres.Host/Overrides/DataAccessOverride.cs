namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Postgres.Host.Overrides;

using CompositionRoot.Registration;
using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
using SpaceEngineers.Core.GenericEndpoint.UnitOfWork;

internal class DataAccessOverride : IComponentsOverride
{
    public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
    {
        container.Override<IIntegrationUnitOfWork, IntegrationUnitOfWork>(EnLifestyle.Scoped);
        container.Override<IOutboxDelivery, OutboxDelivery>(EnLifestyle.Singleton);
    }
}