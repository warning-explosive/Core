namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// EndpointInitializationContext
    /// </summary>
    public class EndpointInitializationContext
    {
        /// <summary> .cctor </summary>
        /// <param name="configuration">IConfiguration</param>
        public EndpointInitializationContext(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }
    }
}