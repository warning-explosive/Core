namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using Api;
    using Api.Abstractions;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
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