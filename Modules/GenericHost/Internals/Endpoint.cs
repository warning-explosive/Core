namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Enumerations;
    using Core.GenericEndpoint;
    using Core.GenericEndpoint.Abstractions;

    internal static class Endpoint
    {
        internal static async Task<IGenericEndpoint> StartAsync(
            EndpointOptions endpointOptions,
            CancellationToken token)
        {
            var dependencyContainer = DependencyContainerPerEndpoint(endpointOptions);

            var endpoint = dependencyContainer.Resolve<IRunnableEndpoint>();
            await endpoint.StartAsync(token).ConfigureAwait(false);

            return dependencyContainer.Resolve<GenericEndpoint>();
        }

        private static IDependencyContainer DependencyContainerPerEndpoint(EndpointOptions endpointOptions)
        {
            var containerOptions = endpointOptions.ContainerOptions ?? new DependencyContainerOptions();

            var registrations = new List<IManualRegistration>(containerOptions.ManualRegistrations)
            {
                new GenericEndpointManualRegistration(endpointOptions.Identity),
                endpointOptions.Transport.Registration
            };

            containerOptions.ManualRegistrations = registrations;

            return endpointOptions.Assembly != null
                ? DependencyContainer.CreateBoundedAbove(endpointOptions.Assembly, containerOptions)
                : DependencyContainer.Create(containerOptions);
        }

        internal class GenericEndpointManualRegistration : IManualRegistration
        {
            private readonly EndpointIdentity _endpointIdentity;

            public GenericEndpointManualRegistration(EndpointIdentity endpointIdentity)
            {
                _endpointIdentity = endpointIdentity;
            }

            public void Register(IRegistrationContainer container)
            {
                container.RegisterInstance(_endpointIdentity);

                container.Register<IGenericEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IRunnableEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IExecutableEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<IMessagePipeline, GenericEndpoint>(EnLifestyle.Singleton);
                container.Register<GenericEndpoint, GenericEndpoint>(EnLifestyle.Singleton);
            }
        }
    }
}