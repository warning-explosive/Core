namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using System;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericHost.Abstractions;
    using GenericHost.Endpoint;

    internal class GenericEndpointRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);

            var assemblyName = $"{nameof(SpaceEngineers)}.{nameof(Core)}.{nameof(GenericHost)}";
            var typeFullName = $"{assemblyName}.{nameof(GenericHost.Endpoint)}.GenericEndpoint";
            var genericEndpointType = AssembliesExtensions
                .FindType(assemblyName, typeFullName)
                .EnsureNotNull($"{typeFullName} must be found");

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