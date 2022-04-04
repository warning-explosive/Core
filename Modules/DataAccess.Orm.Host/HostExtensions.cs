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
    using CompositionRoot.Api.Abstractions.Container;
    using GenericHost;
    using GenericHost.Api;
    using GenericHost.Api.Abstractions;
    using ManualRegistrations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Migrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
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
            hostBuilder.CheckMultipleCalls(nameof(ExecuteMigrations));

            return hostBuilder.ConfigureServices((_, serviceCollection) =>
            {
                serviceCollection.AddSingleton<IHostStartupAction>(serviceProvider =>
                {
                    var endpointDatabaseTypeProviders = serviceProvider
                       .GetServices<DependencyContainer>()
                       .Where(container => container
                           .Resolve<ITypeProvider>()
                           .IsOurType(typeof(IDatabaseTypeProvider)))
                       .Select(container => new EndpointDatabaseTypeProvider(container
                           .Resolve<IDatabaseTypeProvider>()
                           .DatabaseEntities()
                           .ToList()))
                       .ToList();

                    var builder = ConfigureBuilder(hostBuilder, endpointDatabaseTypeProviders);

                    var options = optionsFactory(builder);

                    var dependencyContainer = BuildDependencyContainer(options);

                    return new UpgradeDatabaseHostStartupAction(dependencyContainer);
                });
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
            IReadOnlyCollection<IEndpointDatabaseTypeProvider> endpointDatabaseTypeProviders)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CrossCuttingConcerns)));

            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();

            return new MigrationsEndpointBuilder()
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(new DatabaseTypeProviderManualRegistration(endpointDatabaseTypeProviders))
                   .WithManualRegistrations(new LoggerFactoryManualRegistration(frameworkDependenciesProvider)));
        }
    }
}