namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// WebHostExtensions
    /// </summary>
    public static class WebHostExtensions
    {
        /// <summary>
        /// Use in-memory integration transport inside specified host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="startupFactory">Web application startup factory</param>
        /// <typeparam name="TStartup">TStartup type-argument</typeparam>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseIntegrationTransport<TStartup>(
            this IHostBuilder hostBuilder,
            Func<WebHostBuilderContext, TStartup> startupFactory)
            where TStartup : class, IStartup
        {
            return hostBuilder.ConfigureWebHostDefaults(builder => builder.UseStartup(startupFactory));
        }
    }
}