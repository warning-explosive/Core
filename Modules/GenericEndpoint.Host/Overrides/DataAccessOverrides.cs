namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using DataAccess.Internals;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Implementations;

    internal class DataAccessOverrides : IComponentsOverride
    {
        public void RegisterOverrides(IRegisterComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, IntegrationUnitOfWork, DataAccessIntegrationUnitOfWork>(EnLifestyle.Scoped);
        }
    }
}