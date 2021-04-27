namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using GenericEndpoint.Executable;
    using Microsoft.Extensions.Logging;

    internal static class EndpointOptionsExtensions
    {
        internal static EndpointOptions UseTransport(this EndpointOptions options, IAdvancedIntegrationTransport transport)
        {
            options.ContainerOptions ??= new DependencyContainerOptions();

            options.ContainerOptions.ManualRegistrations =
                new List<IManualRegistration>(options.ContainerOptions.ManualRegistrations)
                {
                    transport.Injection
                };

            return options;
        }

        internal static EndpointOptions UseLogger(this EndpointOptions options, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger(options.Identity.ToString());

            options.ContainerOptions ??= new DependencyContainerOptions();

            options.ContainerOptions.ManualRegistrations =
                new List<IManualRegistration>(options.ContainerOptions.ManualRegistrations)
                {
                    new LoggerManualRegistration(logger)
                };

            return options;
        }
    }
}