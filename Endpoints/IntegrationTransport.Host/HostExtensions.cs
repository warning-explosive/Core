namespace SpaceEngineers.Core.IntegrationTransport.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Basics;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using GenericEndpoint.Contract;
    using GenericHost;
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
        /// <param name="transportIdentity">TransportIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetIntegrationTransportDependencyContainer(
            this IHost host,
            TransportIdentity transportIdentity)
        {
            return host.Services.GetIntegrationTransportDependencyContainer(transportIdentity);
        }

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="transportIdentity">TransportIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetIntegrationTransportDependencyContainer(
            this IServiceProvider serviceProvider,
            TransportIdentity transportIdentity)
        {
            return serviceProvider
                       .GetServices<IntegrationTransportDependencyContainer>()
                       .Select(wrapper => wrapper.DependencyContainer)
                       .Where(IsTransportContainer(transportIdentity))
                       .InformativeSingleOrDefault(Amb(transportIdentity))
                   ?? throw new InvalidOperationException($@".UseIntegrationTransport(""{transportIdentity}"") should be called at least once during host declaration");

            static Func<IDependencyContainer, bool> IsTransportContainer(TransportIdentity transportIdentity)
            {
                return dependencyContainer => dependencyContainer
                    .Resolve<TransportIdentity>()
                    .Equals(transportIdentity);
            }

            static Func<IEnumerable<IDependencyContainer>, string> Amb(TransportIdentity transportIdentity)
            {
                return _ => $"Unable to choose between multiple integration transports with identity: {transportIdentity}";
            }
        }

        /// <summary>
        /// UseInMemoryIntegrationTransport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transportIdentity">TransportIdentity</param>
        /// <param name="optionsFactory">Container options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        // TODO: #225 - review InternalVisibleToAttributes | BeforeAttribute | AfterAttribute
        public static IHostBuilder UseInMemoryIntegrationTransport(
            this IHostBuilder hostBuilder,
            TransportIdentity transportIdentity,
            Func<DependencyContainerOptions, DependencyContainerOptions>? optionsFactory = null)
        {
            hostBuilder.CheckEndpointsExistence();

            return hostBuilder.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(serviceProvider => ConfigureDependencyContainer(serviceProvider, transportIdentity, optionsFactory));

                serviceCollection.AddSingleton(serviceProvider => BuildHostedService(serviceProvider, transportIdentity));
            });

            static IntegrationTransportDependencyContainer ConfigureDependencyContainer(
                IServiceProvider serviceProvider,
                TransportIdentity transportIdentity,
                Func<DependencyContainerOptions, DependencyContainerOptions>? optionsFactory)
            {
                var frameworkDependenciesProvider = serviceProvider.GetRequiredService<IFrameworkDependenciesProvider>();
                var settingsDirectoryProvider = serviceProvider.GetRequiredService<SettingsDirectoryProvider>();
                var hostedServiceRegistry = new HostedServiceRegistry();

                var assemblies = new[]
                {
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(IntegrationTransport), nameof(Api))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(IntegrationTransport), nameof(InMemory)))
                };

                // TODO: #225 - add .WithOpenTelemetry() extension
                var options = new DependencyContainerOptions()
                    .WithPluginAssemblies(assemblies)
                    .WithManualRegistrations(
                        new TransportIdentityManualRegistration(transportIdentity),
                        new SettingsProviderManualRegistration(settingsDirectoryProvider),
                        new LoggerFactoryManualRegistration(transportIdentity, frameworkDependenciesProvider),
                        new HostedServiceRegistryManualRegistration(hostedServiceRegistry),
                        new IntegrationTransportHostedServiceManualRegistration());

                if (optionsFactory != null)
                {
                    options = optionsFactory(options);
                }

                var dependencyContainer = DependencyContainer.Create(options);

                return new IntegrationTransportDependencyContainer(dependencyContainer);
            }
        }

        /// <summary>
        /// UseRabbitMqIntegrationTransport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transportIdentity">TransportIdentity</param>
        /// <param name="optionsFactory">Container options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseRabbitMqIntegrationTransport(
            this IHostBuilder hostBuilder,
            TransportIdentity transportIdentity,
            Func<DependencyContainerOptions, DependencyContainerOptions>? optionsFactory = null)
        {
            hostBuilder.CheckEndpointsExistence();

            return hostBuilder.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(serviceProvider => ConfigureDependencyContainer(serviceProvider, transportIdentity, optionsFactory));

                serviceCollection.AddSingleton(serviceProvider => BuildHostedService(serviceProvider, transportIdentity));
            });

            static IntegrationTransportDependencyContainer ConfigureDependencyContainer(
                IServiceProvider serviceProvider,
                TransportIdentity transportIdentity,
                Func<DependencyContainerOptions, DependencyContainerOptions>? optionsFactory)
            {
                var frameworkDependenciesProvider = serviceProvider.GetRequiredService<IFrameworkDependenciesProvider>();
                var settingsDirectoryProvider = serviceProvider.GetRequiredService<SettingsDirectoryProvider>();
                var hostedServiceRegistry = new HostedServiceRegistry();

                var assemblies = new[]
                {
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CrossCuttingConcerns))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(IntegrationTransport), nameof(Api))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(IntegrationTransport), nameof(RabbitMQ)))
                };

                var options = new DependencyContainerOptions()
                    .WithPluginAssemblies(assemblies)
                    .WithManualRegistrations(
                        new TransportIdentityManualRegistration(transportIdentity),
                        new SettingsProviderManualRegistration(settingsDirectoryProvider),
                        new LoggerFactoryManualRegistration(transportIdentity, frameworkDependenciesProvider),
                        new HostedServiceRegistryManualRegistration(hostedServiceRegistry),
                        new IntegrationTransportHostedServiceManualRegistration());

                if (optionsFactory != null)
                {
                    options = optionsFactory(options);
                }

                var dependencyContainer = DependencyContainer.Create(options);

                return new IntegrationTransportDependencyContainer(dependencyContainer);
            }
        }

        private static IHostedService BuildHostedService(
            IServiceProvider serviceProvider,
            TransportIdentity transportIdentity)
        {
            var dependencyContainer = serviceProvider.GetIntegrationTransportDependencyContainer(transportIdentity);

            return new HostedService(
                transportIdentity.ToString(),
                serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<HostedService>(),
                serviceProvider.GetRequiredService<IHostApplicationLifetime>(),
                dependencyContainer.Resolve<IHostedServiceRegistry>(),
                dependencyContainer.ResolveCollection<IHostedServiceObject>());
        }

        private static void CheckEndpointsExistence(this IHostBuilder hostBuilder)
        {
            if (hostBuilder.TryGetPropertyValue<Dictionary<string, EndpointIdentity>>(nameof(EndpointIdentity), out var endpoints)
                && endpoints.Any())
            {
                throw new InvalidOperationException(".UseIntegrationTransport() should be called before any endpoint declarations");
            }
        }
    }
}