namespace SpaceEngineers.Core.GenericEndpoint.Web.Api.Host
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
        /// Use web-api gateway endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="startupFactory">Web application startup factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseWebApiGateway(
            this IHostBuilder hostBuilder,
            Func<IHostBuilder, Func<WebHostBuilderContext, IStartup>> startupFactory)
        {
            return hostBuilder.ConfigureWebHostDefaults(builder => builder.UseStartup(startupFactory(hostBuilder)));
        }
    }
}