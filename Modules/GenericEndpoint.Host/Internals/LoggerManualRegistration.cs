namespace SpaceEngineers.Core.GenericEndpoint.Host.Internals
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Contract;
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
            container.Advanced.RegisterFactory<ILogger>(() => _loggerFactory.CreateLogger(_endpointIdentity.ToString()), EnLifestyle.Singleton);
        }
    }
}