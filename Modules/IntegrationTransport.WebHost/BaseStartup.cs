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
        /// <summary> .cctor </summary>
        /// <param name="configuration">IConfiguration</param>
        protected BaseStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <inheritdoc />
        public abstract void ConfigureServices(
            IServiceCollection serviceCollection);

        /// <inheritdoc />
        public abstract void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration);
    }
}