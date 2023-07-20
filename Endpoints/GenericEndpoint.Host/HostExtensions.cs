namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Builder;
    using CompositionRoot;
    using Contract;
    using CrossCuttingConcerns.Settings;
    using GenericHost;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Host;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Registrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(
            this IHost host,
            EndpointIdentity endpointIdentity)
        {
            return host.Services.GetEndpointDependencyContainer(endpointIdentity);
        }

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(
            this IServiceProvider serviceProvider,
            EndpointIdentity endpointIdentity)
        {
            return serviceProvider
                       .GetServices<GenericEndpointDependencyContainer>()
                       .Select(wrapper => wrapper.DependencyContainer)
                       .SingleOrDefault(IsEndpointContainer(endpointIdentity))
                   ?? throw new InvalidOperationException($@".UseEndpoint(""{endpointIdentity}"") should be called during host declaration");

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return dependencyContainer => dependencyContainer
                    .Resolve<EndpointIdentity>()
                    .Equals(endpointIdentity);
            }
        }

        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="optionsFactory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
        public static IHostBuilder UseEndpoint(
            this IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity,
            Func<IConfiguration, IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            hostBuilder.CheckDuplicates(endpointIdentity);

            return hostBuilder.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(serviceProvider => ConfigureDependencyContainer(serviceProvider, endpointIdentity, optionsFactory));

                serviceCollection.AddSingleton(serviceProvider => BuildHostedService(serviceProvider, endpointIdentity));
            });

            static GenericEndpointDependencyContainer ConfigureDependencyContainer(
                IServiceProvider serviceProvider,
                EndpointIdentity endpointIdentity,
                Func<IConfiguration, IEndpointBuilder, EndpointOptions> optionsFactory)
            {
                var frameworkDependenciesProvider = serviceProvider.GetRequiredService<IFrameworkDependenciesProvider>();
                var settingsDirectoryProvider = serviceProvider.GetRequiredService<SettingsDirectoryProvider>();
                var hostedServiceRegistry = new HostedServiceRegistry();
                var integrationTransport = GetIntegrationTransport(serviceProvider);

                var assemblies = new[]
                {
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Contract))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Api))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Messaging))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint)))
                };

                var builder = new EndpointBuilder(endpointIdentity)
                   .ModifyContainerOptions(options => options
                       .WithPluginAssemblies(assemblies)
                       .WithManualRegistrations(
                           new GenericEndpointIdentityManualRegistration(endpointIdentity),
                           new SettingsProviderManualRegistration(settingsDirectoryProvider),
                           new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider),
                           new HostedServiceRegistryManualRegistration(hostedServiceRegistry),
                           new IntegrationTransportManualRegistration(integrationTransport),
                           new HostedServiceStartupActionManualRegistration()));

                var options = optionsFactory(serviceProvider.GetRequiredService<IConfiguration>(), builder);

                var dependencyContainer = DependencyContainer.Create(options.ContainerOptions);

                return new GenericEndpointDependencyContainer(dependencyContainer);
            }

            static IIntegrationTransport GetIntegrationTransport(IServiceProvider serviceProvider)
            {
                var dependencyContainer = serviceProvider
                    .GetServices<IntegrationTransportDependencyContainer>()
                    .Select(wrapper => wrapper.DependencyContainer)
                    .InformativeSingleOrDefault(Amb);

                return dependencyContainer.Resolve<IIntegrationTransport>();

                static string Amb(IEnumerable<IDependencyContainer> dependencyContainers)
                {
                    // TODO: #225 - support multiple transport per host
                    return "multiple transports aren't supported";
                }
            }

            static IHostedService BuildHostedService(
                IServiceProvider serviceProvider,
                EndpointIdentity endpointIdentity)
            {
                var dependencyContainer = serviceProvider.GetEndpointDependencyContainer(endpointIdentity);

                return new HostedService(
                    endpointIdentity.ToString(),
                    serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<HostedService>(),
                    serviceProvider.GetRequiredService<IHostApplicationLifetime>(),
                    dependencyContainer.Resolve<IHostedServiceRegistry>(),
                    dependencyContainer.ResolveCollection<IHostedServiceObject>());
            }
        }

        private static void CheckDuplicates(this IHostBuilder hostBuilder, EndpointIdentity endpointIdentity)
        {
            if (!hostBuilder.TryGetPropertyValue<Dictionary<string, EndpointIdentity>>(nameof(EndpointIdentity), out var endpoints))
            {
                endpoints = new Dictionary<string, EndpointIdentity>(StringComparer.OrdinalIgnoreCase);
                hostBuilder.SetPropertyValue(nameof(EndpointIdentity), endpoints);
            }

            if (!endpoints.TryAdd(endpointIdentity.LogicalName, endpointIdentity))
            {
                throw new InvalidOperationException($"Endpoint duplicates was found: {endpointIdentity.LogicalName}. Horizontal scaling in the same process doesn't make sense.");
            }
        }
    }
}