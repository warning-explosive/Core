namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// EndpointInitializationContext
    /// </summary>
    public class EndpointInitializationContext
    {
        /// <summary> .cctor </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="configuration">IConfiguration</param>
        public EndpointInitializationContext(
            IServiceCollection serviceCollection,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            ServiceCollection = serviceCollection;
            ServiceProvider = serviceProvider;
            Configuration = configuration;
        }

        /// <summary>
        /// ServiceCollection
        /// </summary>
        public IServiceCollection ServiceCollection { get; }

        /// <summary>
        /// ServiceProvider
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }
    }
}