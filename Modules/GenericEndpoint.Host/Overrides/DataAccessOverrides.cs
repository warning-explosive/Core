namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using UnitOfWork;

    internal class DataAccessOverrides : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, IntegrationUnitOfWork, GenericEndpoint.DataAccess.UnitOfWork.IntegrationUnitOfWork>(EnLifestyle.Scoped);
        }
    }
}