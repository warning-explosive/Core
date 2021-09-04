namespace SpaceEngineers.Core.GenericEndpoint.Host.Overrides
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using DataAccess.Internals;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Implementations;
    using Implementations;

    internal class DataAccessOverrides : IComponentsOverride
    {
        public void RegisterOverrides(IComponentsOverrideContainer container)
        {
            container.Override<IIntegrationUnitOfWork, IntegrationUnitOfWork, DataAccessIntegrationUnitOfWork>(EnLifestyle.Scoped);
        }
    }
}