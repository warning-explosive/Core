namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using Api;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string RequireBuildHostCall = ".BuildHost() should be called after all endpoint declarations. {0}";
        private const string RequireFrameworkDependenciesProvider = "Unable to find IFrameworkDependenciesProvider.";

        /// <summary>
        /// Gets IFrameworkDependenciesProvider from IHostBuilder
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>IFrameworkDependenciesProvider</returns>
        public static IFrameworkDependenciesProvider GetFrameworkDependenciesProvider(
            this IHostBuilder hostBuilder)
        {
            if (hostBuilder.Properties.TryGetValue(nameof(IFrameworkDependenciesProvider), out var value)
                && value is IFrameworkDependenciesProvider frameworkDependenciesProvider)
            {
                return frameworkDependenciesProvider;
            }

            throw new InvalidOperationException(RequireBuildHostCall.Format(RequireFrameworkDependenciesProvider));
        }

        /// <summary>
        /// Verifies all instances of IDependencyContainer service were registered in core DI container
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        public static void VerifyDependencyContainers(
            this IServiceProvider serviceProvider)
        {
            serviceProvider
               .GetServices<IDependencyContainer>()
               .Distinct()
               .Each(dependencyContainer => ((DependencyContainer)dependencyContainer).Verify());
        }

        /// <summary>
        /// Builds configured host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHost BuildWebHost(this IHostBuilder hostBuilder)
        {
            return hostBuilder.BuildHostInternal();
        }

        /// <summary>
        /// Builds configured host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHost BuildHost(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.BuildHostInternal();

            host.Services.VerifyDependencyContainers();

            return host;
        }

        private static IHost BuildHostInternal(this IHostBuilder hostBuilder)
        {
            hostBuilder.CheckMultipleCalls(nameof(BuildHost));

            var frameworkDependenciesProvider = InitializeFrameworkDependenciesProvider(hostBuilder);

            var host = hostBuilder
                .ConfigureServices((_, serviceCollection) =>
                {
                    serviceCollection.AddSingleton(SetupFrameworkDependenciesProvider(frameworkDependenciesProvider));
                    serviceCollection.AddHostedService(BuildHostedService);
                })
                .Build();

            _ = host.Services.GetRequiredService<IFrameworkDependenciesProvider>();

            return host;

            static FrameworkDependenciesProvider InitializeFrameworkDependenciesProvider(IHostBuilder hostBuilder)
            {
                var frameworkDependenciesProvider = new FrameworkDependenciesProvider();
                hostBuilder.Properties[nameof(IFrameworkDependenciesProvider)] = frameworkDependenciesProvider;
                return frameworkDependenciesProvider;
            }

            static Func<IServiceProvider, IFrameworkDependenciesProvider> SetupFrameworkDependenciesProvider(
                FrameworkDependenciesProvider frameworkDependenciesProvider)
            {
                return serviceProvider =>
                {
                    frameworkDependenciesProvider.SetServiceProvider(serviceProvider);
                    return frameworkDependenciesProvider;
                };
            }

            static HostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new HostedService(
                    Guid.NewGuid(),
                    serviceProvider.GetRequiredService<IHostApplicationLifetime>(),
                    serviceProvider.GetRequiredService<ILoggerFactory>(),
                    serviceProvider.GetServices<IHostStartupAction>(),
                    serviceProvider.GetServices<IHostBackgroundWorker>());
            }
        }
    }
}