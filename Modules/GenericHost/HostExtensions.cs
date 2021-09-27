namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using Api;
    using Api.Abstractions;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string ForbidUseContainerCallMultipleTimes = ".UseContainer() should be called once";

        /// <summary>
        /// UseContainer
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="containerImplementationProducer">Container implementation producer</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseContainer(
            this IHostBuilder hostBuilder,
            Func<DependencyContainerOptions, Func<IDependencyContainerImplementation>> containerImplementationProducer)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseContainer));

            if (!hostBuilder.Properties.TryGetValue(nameof(IDependencyContainerImplementation), out _))
            {
                hostBuilder.Properties[nameof(IDependencyContainerImplementation)] = containerImplementationProducer;
                return hostBuilder;
            }

            throw new InvalidOperationException(ForbidUseContainerCallMultipleTimes);
        }

        /// <summary>
        /// Builds configured host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHost BuildHost(this IHostBuilder hostBuilder)
        {
            hostBuilder.CheckMultipleCalls(nameof(BuildHost));

            return hostBuilder
                .ConfigureServices((_, serviceCollection) =>
                {
                    serviceCollection.AddHostedService(BuildHostedService);
                })
                .Build();

            static HostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new HostedService(
                    serviceProvider.GetRequiredService<ILoggerFactory>(),
                    serviceProvider.GetServices<IHostStartupAction>(),
                    serviceProvider.GetServices<IHostBackgroundWorker>());
            }
        }
    }
}