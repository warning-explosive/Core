namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Contract;
    using GenericHost;
    using GenericHost.Api;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Registrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string EndpointDuplicatesWasFound = "Endpoint duplicates was found: {0}. Horizontal scaling in the same process doesn't make sense.";
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
                       .GetServices<IDependencyContainer>()
                       .SingleOrDefault(IsEndpointContainer(endpointIdentity))
                   ?? throw new InvalidOperationException(RequireUseEndpointCall.Format(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container => ExecutionExtensions
                    .Try(state => IsEndpointContainerUnsafe(state, endpointIdentity), container)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsEndpointContainerUnsafe(IDependencyContainer dependencyContainer, EndpointIdentity endpointIdentity)
            {
                return dependencyContainer
                   .Resolve<EndpointIdentity>()
                   .Equals(endpointIdentity);
            }
        }

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="logicalName">Logical name</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(this IHost host, string logicalName)
        {
            return host
                       .Services
                       .GetServices<IDependencyContainer>()
                       .SingleOrDefault(IsEndpointContainer(logicalName))
                   ?? throw new InvalidOperationException(RequireUseEndpointCall.Format(logicalName));

            static Func<IDependencyContainer, bool> IsEndpointContainer(string logicalName)
            {
                return container => ExecutionExtensions
                    .Try(state => IsEndpointContainerUnsafe(state, logicalName), container)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsEndpointContainerUnsafe(IDependencyContainer dependencyContainer, string logicalName)
            {
                return dependencyContainer
                   .Resolve<EndpointIdentity>()
                   .LogicalName
                   .Equals(logicalName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="optionsFactory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseEndpoint(
            this IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity,
            Func<HostBuilderContext, IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            hostBuilder.CheckDuplicates(endpointIdentity);

            return hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                var builder = ConfigureBuilder(hostBuilder, endpointIdentity);

                var options = optionsFactory(context, builder);

                var dependencyContainer = BuildDependencyContainer(options);

                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);
            });
        }

        internal static void CheckDuplicates(this IHostBuilder hostBuilder, EndpointIdentity endpointIdentity)
        {
            if (!hostBuilder.TryGetPropertyValue<Dictionary<string, EndpointIdentity>>(nameof(EndpointIdentity), out var endpoints))
            {
                endpoints = new Dictionary<string, EndpointIdentity>(StringComparer.OrdinalIgnoreCase);
                hostBuilder.SetPropertyValue(nameof(EndpointIdentity), endpoints);
            }

            if (!endpoints.TryAdd(endpointIdentity.LogicalName, endpointIdentity))
            {
                throw new InvalidOperationException(EndpointDuplicatesWasFound.Format(endpointIdentity.LogicalName));
            }
        }

        private static IDependencyContainer BuildDependencyContainer(EndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }

        private static IEndpointBuilder ConfigureBuilder(
            IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(CrossCuttingConcerns)));

            var settingsDirectoryProvider = hostBuilder.GetSettingsDirectoryProvider();
            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();
            var plugins = hostBuilder.GetPlugins();
            var injections = hostBuilder.GetInjections();

            return new EndpointBuilder(endpointIdentity)
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .WithEndpointPluginAssemblies(plugins)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(
                       new GenericEndpointIdentityManualRegistration(endpointIdentity),
                       new SettingsProviderManualRegistration(settingsDirectoryProvider),
                       new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider),
                       new HostStartupActionsRegistryManualRegistration(frameworkDependenciesProvider),
                       new GenericEndpointHostStartupActionManualRegistration())
                   .WithManualRegistrations(injections)
                   .WithManualVerification(true));
        }

        private static Assembly[] GetPlugins(this IHostBuilder hostBuilder)
        {
            return hostBuilder.TryGetPropertyValue<Assembly[]>(nameof(Assembly), out var plugins)
                ? plugins
                : Array.Empty<Assembly>();
        }

        private static IManualRegistration[] GetInjections(this IHostBuilder hostBuilder)
        {
            return hostBuilder.TryGetPropertyValue<IManualRegistration[]>(nameof(IManualRegistration), out var injections)
                ? injections
                : Array.Empty<IManualRegistration>();
        }
    }
}