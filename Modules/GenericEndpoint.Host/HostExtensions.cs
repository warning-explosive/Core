namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using CompositionRoot.Api.Extensions;
    using Contract;
    using GenericHost.Api.Abstractions;
    using Implementations;
    using IntegrationTransport.Api.Abstractions;
    using ManualRegistrations;
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
                .Single(IsTransportContainer);

            static bool IsTransportContainer(IDependencyContainer container)
            {
                return ExecutionExtensions
                    .Try(container, IsTransportContainerUnsafe)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsTransportContainerUnsafe(IDependencyContainer container)
            {
                var endpointIdentity = container.Resolve<EndpointIdentity>();
                var integrationTransport = container.Resolve<IIntegrationTransport>();

                return integrationTransport
                    .FlattenDecoratedType()
                    .Any(it => it.Name.Equals(
                        endpointIdentity.LogicalName,
                        StringComparison.OrdinalIgnoreCase));
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
            return GetEndpointDependencyContainer(host.Services, endpointIdentity);
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
            var dependencyContainer = GetEndpointDependencyContainer(serviceProvider, endpointOptions.Identity);
            return new GenericEndpointHostStartupAction(dependencyContainer);
        }

        private static IDependencyContainer GetEndpointDependencyContainer(IServiceProvider serviceProvider, EndpointIdentity endpointIdentity)
        {
            return serviceProvider
                .GetServices<IDependencyContainer>()
                .Single(IsEndpointContainer(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container => ExecutionExtensions
                    .Try(endpointIdentity, IsEndpointContainerUnsafe(container))
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static Func<EndpointIdentity, bool> IsEndpointContainerUnsafe(IDependencyContainer container)
            {
                return endpointIdentity => container.Resolve<EndpointIdentity>().Equals(endpointIdentity);
            }
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