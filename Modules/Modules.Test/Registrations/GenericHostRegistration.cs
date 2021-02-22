namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using Basics;
    using GenericHost;
    using GenericHost.Abstractions;
    using GenericHost.Defaults;

    internal class GenericHostRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            string assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericHost));
            string typeFullName = AssembliesExtensions.BuildName(assemblyName, "Transport", "InMemoryIntegrationTransport");
            var transportType = AssembliesExtensions.FindRequiredType(assemblyName, typeFullName);
            container.Register(typeof(IIntegrationTransport), transportType, EnLifestyle.Singleton);
            container.Register(transportType, transportType, EnLifestyle.Singleton);

            container.Register<IEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
            container.Register<DefaultEndpointInstanceSelectionBehavior, DefaultEndpointInstanceSelectionBehavior>(EnLifestyle.Singleton);
        }
    }
}