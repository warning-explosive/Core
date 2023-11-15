namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// IStartup
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// This method gets called by the runtime.
        /// Use this method to add services to the container.
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        void ConfigureServices(IServiceCollection serviceCollection);

        /// <summary>
        /// This method gets called by the runtime.
        /// Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="applicationBuilder">IApplicationBuilder</param>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <param name="configuration">IConfiguration</param>
        void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration);
    }
}