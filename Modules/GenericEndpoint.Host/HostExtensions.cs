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
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Overrides;
    using StartupActions;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string EndpointDuplicatesWasFound = "Endpoint duplicates was found: {0}";
        private const string RequireUseTransportCall = ".UseIntegrationTransport() should be called before any endpoint declarations. {0}";
        private const string RequireTransportInjection = "Unable to find IIntegrationTransport injection.";
        private const string RequireUseEndpointCall = ".UseEndpoint() with identity {0} should be called during host declaration";

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

                var endpointDependencyContainer = BuildEndpointContainer(ConfigureEndpointOptions(hostBuilder, endpointOptions));

                serviceCollection.AddSingleton<DependencyContainer>(endpointDependencyContainer);

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(producer(endpointDependencyContainer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(producer(endpointDependencyContainer));
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
            IHostBuilder hostBuilder,
            EndpointOptions endpointOptions)
        {
            var integrationTransportInjection = hostBuilder.GetIntegrationTransportInjection();

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            var containerOptions = endpointOptions.ContainerOptions
                .WithManualRegistrations(integrationTransportInjection)
                .WithManualRegistrations(new GenericEndpointIdentityManualRegistration(endpointOptions.Identity))
                .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointOptions.Identity, frameworkDependenciesProvider))
                .WithOverrides(new SettingsProviderOverrides())
                .WithManualVerification(true);

            return endpointOptions.WithContainerOptions(containerOptions);
        }

        private static IManualRegistration GetIntegrationTransportInjection(this IHostBuilder hostBuilder)
        {
            if (hostBuilder.Properties.TryGetValue(nameof(IIntegrationTransport), out var value)
                && value is IManualRegistration injection)
            {
                return injection;
            }

            throw new InvalidOperationException(RequireUseTransportCall.Format(RequireTransportInjection));
        }
    }
}