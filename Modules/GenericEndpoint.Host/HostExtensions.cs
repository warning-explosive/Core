namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Extensions;
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
        private const string RequireUseTransportCall = ".UseTransport() should be called before any endpoint declarations";
        private const string RequireUseContainerCall = ".UseContainer() should be called before any endpoint declarations";
        private const string EndpointDuplicatesWasFound = "Endpoint duplicates was found: {0}";

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

                            return integrationTransport
                                .FlattenDecoratedType()
                                .Any(it => it.Name.Equals(
                                    endpointIdentity.LogicalName,
                                    StringComparison.OrdinalIgnoreCase));
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
        /// <param name="factory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseEndpoint(
            this IHostBuilder hostBuilder,
            Func<IEndpointBuilder, EndpointOptions> factory)
        {
            var endpointOptions = factory(new EndpointBuilder());

            hostBuilder.ApplyOptions(endpointOptions);

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                serviceCollection.AddSingleton<IDependencyContainer>(serviceProvider => BuildEndpointContainer(ctx, ConfigureEndpointOptions(ctx, serviceProvider, endpointOptions)));
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
                throw new InvalidOperationException(EndpointDuplicatesWasFound.Format(string.Join(", ", duplicates)));
            }

            optionsCollection.Add(endpointOptions);
        }

        private static IDependencyContainer BuildEndpointContainer(
            HostBuilderContext context,
            EndpointOptions endpointOptions)
        {
            var containerImplementationProducer = GetContainerImplementationProducer(context);

            return DependencyContainer.CreateBoundedAbove(
                endpointOptions.ContainerOptions,
                containerImplementationProducer(endpointOptions.ContainerOptions),
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
                .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointOptions.Identity, loggerFactory));

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
            if (ctx.Properties.TryGetValue(GenericHost.Api.HostExtensions.TransportInjectionKey, out var value)
                && value is IManualRegistration transportInjection)
            {
                return transportInjection;
            }

            throw new InvalidOperationException(RequireUseTransportCall);
        }

        private static Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> GetContainerImplementationProducer(HostBuilderContext context)
        {
            if (context.Properties.TryGetValue(nameof(IDependencyContainerImplementation), out var value)
                && value is Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer)
            {
                return containerImplementationProducer;
            }

            throw new InvalidOperationException(RequireUseContainerCall);
        }
    }
}