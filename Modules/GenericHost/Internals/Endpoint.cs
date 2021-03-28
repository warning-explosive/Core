namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
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

            return DependencyContainer.CreateBoundedAbove(containerOptions, endpointOptions.AboveAssemblies.ToArray());
        }

        internal class GenericEndpointManualRegistration : IManualRegistration
        {
            private readonly EndpointIdentity _endpointIdentity;

            public GenericEndpointManualRegistration(EndpointIdentity endpointIdentity)
            {
                _endpointIdentity = endpointIdentity;
            }

            public void Register(IManualRegistrationsContainer container)
            {
                container.RegisterInstance(_endpointIdentity);

                container.Register<IGenericEndpoint, GenericEndpoint>();
                container.Register<IRunnableEndpoint, GenericEndpoint>();
                container.Register<IExecutableEndpoint, GenericEndpoint>();
                container.Register<IMessagePipeline, GenericEndpoint>();
                container.Register<GenericEndpoint, GenericEndpoint>();
            }
        }
    }
}