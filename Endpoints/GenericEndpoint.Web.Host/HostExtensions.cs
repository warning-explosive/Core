namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Use web-gateway endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="startupFactory">Web application startup factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseWebGateway(
            this IHostBuilder hostBuilder,
            Func<IHostBuilder, Func<WebHostBuilderContext, IStartup>> startupFactory)
        {
            return hostBuilder.ConfigureWebHostDefaults(builder => builder.UseStartup(startupFactory(hostBuilder)));
        }
    }
}