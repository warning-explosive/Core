namespace SpaceEngineers.Core.GenericHost.Internals
{
    using Abstractions;
    using CrossCuttingConcerns;
    using GenericEndpoint.Executable;
    using Microsoft.Extensions.Logging;

    internal static class EndpointOptionsExtensions
    {
        internal static EndpointOptions UseTransport(this EndpointOptions options, IAdvancedIntegrationTransport transport)
        {
            options.ContainerOptions.WithManualRegistration(transport.Injection);
            return options;
        }

        internal static EndpointOptions UseLogger(this EndpointOptions options, ILoggerFactory loggerFactory)
        {
            options.ContainerOptions.WithManualRegistration(new LoggerManualRegistration(options.Identity, loggerFactory));
            return options;
        }

        internal static EndpointOptions UseCrossCuttingConcerns(this EndpointOptions options)
        {
            options.ContainerOptions.WithManualRegistration(new CrossCuttingConcernsManualRegistration());
            return options;
        }
    }
}