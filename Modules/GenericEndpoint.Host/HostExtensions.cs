namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using Contract;
    using GenericHost.Api.Abstractions;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Overrides;
    using StartupActions;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string EndpointDuplicatesWasFound = "Endpoint duplicates was found: {0}";
        private const string RequireUseTransportCall = ".UseIntegrationTransport() should be called before any endpoint declarations";
        private const string RequireUseEndpointCall = ".UseEndpoint() with identity {0} should be called during host declaration";

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
            Func<HostBuilderContext, IEndpointBuilder, EndpointOptions> factory)
        {
            return hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                var builder = new EndpointBuilder().WithStartupAction(dependencyContainer => new GenericEndpointHostStartupAction(dependencyContainer));

                var endpointOptions = factory(context, builder);

                hostBuilder.ApplyOptions(endpointOptions);

                serviceCollection.AddSingleton<DependencyContainer>(serviceProvider => BuildEndpointContainer(ConfigureEndpointOptions(serviceProvider, endpointOptions)));

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(serviceProvider => BuildEndpointStartupAction(serviceProvider, endpointOptions, producer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(serviceProvider => BuildEndpointBackgroundWorker(serviceProvider, endpointOptions, producer));
                }
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

        private static DependencyContainer BuildEndpointContainer(
            EndpointOptions endpointOptions)
        {
            return (DependencyContainer)DependencyContainer.CreateBoundedAbove(
                endpointOptions.ContainerOptions,
                endpointOptions.ContainerImplementationProducer(endpointOptions.ContainerOptions),
                endpointOptions.AboveAssemblies.ToArray());
        }

        private static EndpointOptions ConfigureEndpointOptions(
            IServiceProvider serviceProvider,
            EndpointOptions endpointOptions)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var integrationTransportInjection = GetTransportEndpointDependencyContainer(serviceProvider).Resolve<IManualRegistration>()
                                                ?? throw new InvalidOperationException(RequireUseTransportCall);

            var containerOptions = endpointOptions.ContainerOptions
                .WithManualRegistrations(integrationTransportInjection)
                .WithManualRegistrations(new GenericEndpointIdentityManualRegistration(endpointOptions.Identity))
                .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointOptions.Identity, loggerFactory))
                .WithOverrides(new SettingsProviderOverrides());

            return endpointOptions.WithContainerOptions(containerOptions);
        }

        private static IHostStartupAction BuildEndpointStartupAction(
            IServiceProvider serviceProvider,
            EndpointOptions endpointOptions,
            Func<IDependencyContainer, IHostStartupAction> producer)
        {
            var dependencyContainer = GetEndpointDependencyContainer(serviceProvider, endpointOptions.Identity);
            return producer(dependencyContainer);
        }

        private static IHostBackgroundWorker BuildEndpointBackgroundWorker(
            IServiceProvider serviceProvider,
            EndpointOptions endpointOptions,
            Func<IDependencyContainer, IHostBackgroundWorker> producer)
        {
            var dependencyContainer = GetEndpointDependencyContainer(serviceProvider, endpointOptions.Identity);
            return producer(dependencyContainer);
        }

        private static IDependencyContainer GetEndpointDependencyContainer(
            IServiceProvider serviceProvider,
            EndpointIdentity endpointIdentity)
        {
            return serviceProvider
                       .GetServices<DependencyContainer>()
                       .SingleOrDefault(IsEndpointContainer(endpointIdentity))
                   ?? throw new InvalidOperationException(RequireUseEndpointCall.Format(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container => ExecutionExtensions
                    .Try(endpointIdentity, IsEndpointContainerUnsafe(container))
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static Func<EndpointIdentity, bool> IsEndpointContainerUnsafe(IDependencyContainer dependencyContainer)
            {
                return endpointIdentity => dependencyContainer.Resolve<EndpointIdentity>().Equals(endpointIdentity);
            }
        }

        private static IDependencyContainer GetTransportEndpointDependencyContainer(IServiceProvider serviceProvider)
        {
            return serviceProvider
                .GetServices<IDependencyContainer>()
                .SingleOrDefault(IsTransportContainer)
                ?? throw new InvalidOperationException(RequireUseTransportCall);

            static bool IsTransportContainer(IDependencyContainer dependencyContainer)
            {
                return ExecutionExtensions
                    .Try(dependencyContainer, IsTransportContainerUnsafe)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsTransportContainerUnsafe(IDependencyContainer dependencyContainer)
            {
                var endpointIdentity = dependencyContainer.Resolve<EndpointIdentity>();

                return endpointIdentity.LogicalName.Equals(nameof(IntegrationTransport), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}