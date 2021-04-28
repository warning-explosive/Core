namespace SpaceEngineers.Core.GenericHost.Internals
{
    using AutoRegistration.Abstractions;
    using GenericEndpoint;
    using Microsoft.Extensions.Logging;

    internal class LoggerManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly ILoggerFactory _loggerFactory;

        public LoggerManualRegistration(EndpointIdentity endpointIdentity, ILoggerFactory loggerFactory)
        {
            _endpointIdentity = endpointIdentity;
            _loggerFactory = loggerFactory;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            var logger = _loggerFactory.CreateLogger(_endpointIdentity.ToString());

            container.RegisterInstance<ILogger>(logger);
        }
    }
}