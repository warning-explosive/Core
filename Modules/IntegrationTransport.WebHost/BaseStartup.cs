namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// BaseStartup
    /// </summary>
    public abstract class BaseStartup : IStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            ConfigureApplicationServices(serviceCollection);
            ConfigureCoreServices(serviceCollection);
        }

        /// <inheritdoc />
        public void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            ConfigureRequestPipeline(applicationBuilder, environment, configuration);
            ConfigureApplicationServices(applicationBuilder, environment, configuration);
        }

        /// <summary>
        /// Configure core container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        protected abstract void ConfigureCoreServices(IServiceCollection serviceCollection);

        /// <summary>
        /// Configure application container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        protected abstract void ConfigureApplicationServices(IServiceCollection serviceCollection);

        /// <summary>
        /// Configure request pipeline
        /// </summary>
        /// <param name="applicationBuilder">IApplicationBuilder</param>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <param name="configuration">IConfiguration</param>
        protected abstract void ConfigureRequestPipeline(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration);

        /// <summary>
        /// Configure application container
        /// </summary>
        /// <param name="applicationBuilder">IApplicationBuilder</param>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <param name="configuration">IConfiguration</param>
        protected abstract void ConfigureApplicationServices(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration);
    }
}