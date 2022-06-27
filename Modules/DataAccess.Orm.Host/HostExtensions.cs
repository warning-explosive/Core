namespace SpaceEngineers.Core.DataAccess.Orm.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Model;
    using Basics;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericHost;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using GenericHost.Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Migrations;
    using Registrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string RequireExecuteMigrationsCall = ".ExecuteMigrations() should be called after all endpoint declarations. {0}";
        private const string RequireMigrationsDependencyContainer = "Unable to resolve migrations IDependencyContainer.";

        /// <summary>
        /// Gets migrations dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <returns>Migrations dependency container</returns>
        public static IDependencyContainer GetMigrationsDependencyContainer(
            this IHost host)
        {
            return host
               .Services
               .GetServices<IDependencyContainer>()
               .SingleOrDefault(IsTransportContainer)
               .EnsureNotNull(RequireExecuteMigrationsCall.Format(RequireMigrationsDependencyContainer));

            static bool IsTransportContainer(IDependencyContainer container)
            {
                return ExecutionExtensions
                   .Try(container, IsTransportContainerUnsafe)
                   .Catch<Exception>()
                   .Invoke(_ => false);
            }

            static bool IsTransportContainerUnsafe(IDependencyContainer container)
            {
                return container
                   .Resolve<EndpointIdentity>()
                   .LogicalName
                   .Equals(MigrationsEndpointIdentity.LogicalName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Configures generic host so as to execute database migrations
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="optionsFactory">Migrations endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder ExecuteMigrations(
            this IHostBuilder hostBuilder,
            Func<IMigrationsEndpointBuilder, MigrationsEndpointOptions> optionsFactory)
        {
            hostBuilder.CheckMultipleCalls(MigrationsEndpointIdentity.LogicalName);

            return hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                if (!hostBuilder.Properties.TryGetValue(nameof(EndpointIdentity), out var value)
                 || value is not ICollection<EndpointIdentity> optionsCollection)
                {
                    throw new InvalidOperationException();
                }

                var producers = optionsCollection
                   .Select(it => new Func<IFrameworkDependenciesProvider, IDependencyContainer>(provider =>
                        provider
                           .GetServices<IDependencyContainer>()
                           .Single(container => container.Resolve<ITypeProvider>().IsOurType(typeof(IDatabaseTypeProvider))
                                             && container.Resolve<EndpointIdentity>().Equals(it))))
                   .ToList();

                var builder = ConfigureBuilder(hostBuilder, producers);

                var options = optionsFactory(builder);

                var dependencyContainer = BuildDependencyContainer(options);

                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);

                serviceCollection.AddSingleton<IHostStartupAction>(new UpgradeDatabaseHostStartupAction(dependencyContainer));
            });
        }

        private static IDependencyContainer BuildDependencyContainer(MigrationsEndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }

        private static IMigrationsEndpointBuilder ConfigureBuilder(
            IHostBuilder hostBuilder,
            IReadOnlyCollection<Func<IFrameworkDependenciesProvider, IDependencyContainer>> producers)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CrossCuttingConcerns)));

            var endpointIdentity = new EndpointIdentity(MigrationsEndpointIdentity.LogicalName, Guid.NewGuid());

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            return new MigrationsEndpointBuilder()
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new MigrationsEndpointIdentityManualRegistration(endpointIdentity))
                   .WithManualRegistrations(new DatabaseTypeProviderManualRegistration(frameworkDependenciesProvider, producers))
                   .WithManualRegistrations(new LoggerFactoryManualRegistration(frameworkDependenciesProvider))
                   .WithManualRegistrations(new ConfigurationProviderManualRegistration())
                   .WithManualVerification(true));
        }
    }
}