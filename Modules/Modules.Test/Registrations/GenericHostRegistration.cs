namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using Basics;
    using GenericEndpoint.Abstractions;
    using GenericHost;
    using GenericHost.Abstractions;
    using GenericHost.Defaults;

    internal class GenericHostRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericHost));

            var transportTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Transport", "InMemoryIntegrationTransport");
            var transportType = AssembliesExtensions.FindRequiredType(assemblyName, transportTypeFullName);
            container.Register(typeof(IIntegrationTransport), transportType, EnLifestyle.Singleton);
            container.Register(transportType, transportType, EnLifestyle.Singleton);

            var contextTypeFullName = AssembliesExtensions.BuildName(assemblyName, "Transport", "InMemoryIntegrationContext");
            var contextType = AssembliesExtensions.FindRequiredType(assemblyName, contextTypeFullName);
            container.Register(typeof(IIntegrationContext), contextType, EnLifestyle.Scoped);
            container.Register(typeof(IExtendedIntegrationContext), contextType, EnLifestyle.Scoped);
            container.Register(contextType, contextType, EnLifestyle.Scoped);

            container.Register<IEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
            container.Register<DefaultEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
        }
    }
}