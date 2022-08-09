namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Registration;
    using Contract;
    using GenericHost;
    using GenericHost.Api.Abstractions;
    using GenericHost.Internals;
    using IntegrationTransport.Api.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Overrides;
    using Registrations;
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
        private const string RequireExecuteMigrationsCall = ".ExecuteMigrations() should be called after all endpoint declarations {0}";
        private const string RequireUseEndpointCallBeforeMigrationsCall = ".UseEndpoint() with identity {0} should be called before migrations declaration";

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
                .EnsureNotNull(RequireUseEndpointCall.Format(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container => ExecutionExtensions
                    .Try(container, state => IsEndpointContainerUnsafe(state, endpointIdentity))
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
            hostBuilder.ApplyOptions(endpointIdentity);

            return hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                var builder = ConfigureBuilder(hostBuilder, endpointIdentity);

                var options = optionsFactory(context, builder);

                var dependencyContainer = BuildDependencyContainer(options);

                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);

                foreach (var producer in builder.StartupActions)
                {
                    serviceCollection.AddSingleton<IHostStartupAction>(producer(dependencyContainer));
                }

                foreach (var producer in builder.BackgroundWorkers)
                {
                    serviceCollection.AddSingleton<IHostBackgroundWorker>(producer(dependencyContainer));
                }
            });
        }

        private static void ApplyOptions(this IHostBuilder hostBuilder, EndpointIdentity endpointIdentity)
        {
            if (hostBuilder.Properties.Keys.OfType<string>().Contains("Migrations", StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(RequireExecuteMigrationsCall.Format(RequireUseEndpointCallBeforeMigrationsCall.Format(endpointIdentity)));
            }

            if (!hostBuilder.Properties.TryGetValue(nameof(EndpointIdentity), out var value)
                || value is not ICollection<EndpointIdentity> identities)
            {
                hostBuilder.Properties[nameof(EndpointIdentity)] = new List<EndpointIdentity> { endpointIdentity };
                return;
            }

            var duplicates = identities
                .Concat(new[] { endpointIdentity })
                .GroupBy(identity => identity)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.ToString())
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException(EndpointDuplicatesWasFound.Format(string.Join(", ", duplicates)));
            }

            identities.Add(endpointIdentity);
        }

        private static IDependencyContainer BuildDependencyContainer(EndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }

        private static IEndpointBuilder ConfigureBuilder(IHostBuilder hostBuilder, EndpointIdentity endpointIdentity)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CrossCuttingConcerns)));

            var integrationTransportInjection = hostBuilder.GetIntegrationTransportInjection();

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            return new EndpointBuilder(endpointIdentity)
               .WithStartupAction(dependencyContainer => new GenericEndpointHostStartupAction(dependencyContainer))
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(integrationTransportInjection)
                   .WithManualRegistrations(new GenericEndpointIdentityManualRegistration(endpointIdentity))
                   .WithManualRegistrations(new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider))
                   .WithManualRegistrations(new ConfigurationProviderManualRegistration())
                   .WithOverrides(new SettingsProviderOverride())
                   .WithManualVerification(true));
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