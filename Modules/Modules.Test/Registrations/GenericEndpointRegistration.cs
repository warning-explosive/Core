namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    internal class GenericEndpointRegistration : IManualRegistration
    {
        public void Register(IRegistrationContainer container)
        {
            var endpointIdentity = new EndpointIdentity("mock_endpoint", 0);

            var genericHostAssemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericHost));

            var genericEndpointTypeFullName = AssembliesExtensions.BuildName(genericHostAssemblyName, "Internals", "GenericEndpoint");
            var genericEndpointType = AssembliesExtensions.FindRequiredType(genericHostAssemblyName, genericEndpointTypeFullName);

            var runnableEndpointTypeFullName = AssembliesExtensions.BuildName(genericHostAssemblyName, "Internals", "IRunnableEndpoint");
            var runnableEndpointType = AssembliesExtensions.FindRequiredType(genericHostAssemblyName, runnableEndpointTypeFullName);

            var executableEndpointTypeFullName = AssembliesExtensions.BuildName(genericHostAssemblyName, "Internals", "IExecutableEndpoint");
            var executableEndpointType = AssembliesExtensions.FindRequiredType(genericHostAssemblyName, executableEndpointTypeFullName);

            container.RegisterInstance(endpointIdentity);
            container.Register(typeof(IGenericEndpoint), genericEndpointType, EnLifestyle.Singleton);
            container.Register(runnableEndpointType, genericEndpointType, EnLifestyle.Singleton);
            container.Register(executableEndpointType, genericEndpointType, EnLifestyle.Singleton);
            container.Register(typeof(IMessagePipeline), genericEndpointType, EnLifestyle.Singleton);
            container.Register(genericEndpointType, genericEndpointType, EnLifestyle.Singleton);
        }
    }
}