namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Contract;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetTransportDependencyContainer(this IHost host)
        {
            return host
                .Services
                .GetServices<IDependencyContainer>()
                .Single(IsTransportContainer());

            static Func<IDependencyContainer, bool> IsTransportContainer()
            {
                return container =>
                    new Func<bool>(() =>
                        {
                            var endpointIdentity = container.Resolve<EndpointIdentity>();
                            var integrationTransport = container.Resolve<IIntegrationTransport>();

                            // TODO: unwrap decorators
                            return endpointIdentity.LogicalName.Equals(
                                integrationTransport.GetType().Name,
                                StringComparison.OrdinalIgnoreCase);
                        })
                        .Try()
                        .Catch<Exception>()
                        .Invoke(_ => false);
            }
        }

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(this IHost host, EndpointIdentity endpointIdentity)
        {
            return host
                .Services
                .GetServices<IDependencyContainer>()
                .Single(IsEndpointContainer(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container =>
                    new Func<bool>(() => container.Resolve<EndpointIdentity>().Equals(endpointIdentity))
                        .Try()
                        .Catch<Exception>()
                        .Invoke(_ => false);
            }
        }

        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="implementationProducer">Dependency container implementation producer</param>
        /// <param name="factory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseEndpoint(
            this IHostBuilder hostBuilder,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> implementationProducer,
            Func<IEndpointBuilder, EndpointOptions> factory)
        {
            var endpointOptions = factory(new EndpointBuilder());

            hostBuilder.ApplyOptions(endpointOptions);

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(serviceProvider => BuildEndpointContainer(ConfigureEndpointOptions(ctx, serviceProvider, endpointOptions), implementationProducer));
                serviceCollection.AddSingleton<IHostStartupAction>(serviceProvider => BuildStartup(serviceProvider, endpointOptions));
            });
        }

        private static void ApplyOptions(this IHostBuilder hostBuilder, EndpointOptions endpointOptions)
        {
            if (!hostBuilder.Properties.TryGetValue(nameof(EndpointOptions), out var value)
                || value is not ICollection<EndpointOptions> optionsCollection)
            {
                hostBuilder.Properties[nameof(EndpointOptions)] = new List<EndpointOptions> { endpointOptions };
                return;
            }

            var duplicates = optionsCollection
                .Concat(new[] { endpointOptions })
                .GroupBy(e => e.Identity)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.ToString())
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException(
                    $"Endpoint duplicates was found: {string.Join(", ", duplicates)}");
            }

            optionsCollection.Add(endpointOptions);
        }

        private static IDependencyContainer BuildEndpointContainer(
            EndpointOptions endpointOptions,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> implementationProducer)
        {
            return DependencyContainer.CreateBoundedAbove(
                endpointOptions.ContainerOptions,
                implementationProducer(endpointOptions.ContainerOptions),
                endpointOptions.AboveAssemblies.ToArray());
        }

        private static EndpointOptions ConfigureEndpointOptions(
            HostBuilderContext ctx,
            IServiceProvider serviceProvider,
            EndpointOptions endpointOptions)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var integrationTransportInjection = ctx.GetTransportInjection();

            var containerOptions = endpointOptions.ContainerOptions
                .WithManualRegistrations(integrationTransportInjection)
                .WithManualRegistrations(new GenericEndpointIdentityManualRegistration(endpointOptions.Identity))
                .WithManualRegistrations(new LoggerManualRegistration(endpointOptions.Identity, loggerFactory));

            return endpointOptions.WithContainerOptions(containerOptions);
        }

        private static IHostStartupAction BuildStartup(IServiceProvider serviceProvider, EndpointOptions endpointOptions)
        {
            var dependencyContainer = serviceProvider
                .GetServices<IDependencyContainer>()
                .Single(container =>
                {
                    return new Func<bool>(() => container.Resolve<EndpointIdentity>().Equals(endpointOptions.Identity))
                        .Try()
                        .Catch<Exception>()
                        .Invoke(_ => false);
                });

            return new GenericEndpointHostStartupAction(dependencyContainer);
        }

        private static IManualRegistration GetTransportInjection(this HostBuilderContext ctx)
        {
            if (ctx.Properties.TryGetValue(GenericHost.Api.HostExtensions.TransportInjectionKey, out var value))
            {
                return value.EnsureNotNull<IManualRegistration>(".UseTransport() should be called before any endpoint declarations");
            }

            throw new InvalidOperationException("You should call .UseTransport() extension method");
        }
    }
}