namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericHost;

    internal class GenericEndpointRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);

            var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericHost));
            var typeFullName = AssembliesExtensions.BuildName(assemblyName, "Internals", "GenericEndpoint");
            var genericEndpointType = AssembliesExtensions.FindRequiredType(assemblyName, typeFullName);

            var options = new EndpointOptions(endpointIdentity)
            {
                Assembly = typeof(IGenericEndpoint).Assembly
            };

            var genericEndpointInstance = Activator
                .CreateInstance(genericEndpointType, options)
                .EnsureNotNull($"{typeFullName} must be instantiated");

            container.Register<EndpointIdentity>(() => endpointIdentity, EnLifestyle.Singleton);
            container.Register(typeof(IMessagePipeline), () => genericEndpointInstance, EnLifestyle.Singleton);
            container.Register(genericEndpointType, () => genericEndpointInstance, EnLifestyle.Singleton);
        }
    }
}