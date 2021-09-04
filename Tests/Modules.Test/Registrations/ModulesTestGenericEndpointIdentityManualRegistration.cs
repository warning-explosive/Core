namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Host.ManualRegistrations;

    internal class ModulesTestGenericEndpointIdentityManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var name = AssembliesExtensions.BuildName(nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test));
            var endpointIdentity = new EndpointIdentity(name, 0);
            new GenericEndpointIdentityManualRegistration(endpointIdentity).Register(container);
        }
    }
}