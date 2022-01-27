namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using Basics;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host.ManualRegistrations;

    internal class ModulesTestGenericEndpointIdentityManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var logicalName = AssembliesExtensions.BuildName(nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test));
            var endpointIdentity = new EndpointIdentity(logicalName, 0);
            new GenericEndpointIdentityManualRegistration(endpointIdentity).Register(container);
        }
    }
}