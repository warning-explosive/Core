namespace SpaceEngineers.Core.GenericHost.Internals
{
    using AutoRegistration.Abstractions;
    using Microsoft.Extensions.Logging;

    internal class LoggerManualRegistration : IManualRegistration
    {
        private readonly ILogger _logger;

        public LoggerManualRegistration(ILogger logger)
        {
            _logger = logger;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance<ILogger>(_logger);
        }
    }
}