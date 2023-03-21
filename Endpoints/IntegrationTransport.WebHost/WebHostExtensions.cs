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
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseIntegrationTransport(
            this IHostBuilder hostBuilder,
            Func<IHostBuilder, Func<WebHostBuilderContext, IStartup>> startupFactory)
        {
            return hostBuilder.ConfigureWebHostDefaults(builder => builder.UseStartup(startupFactory(hostBuilder)));
        }
    }
}