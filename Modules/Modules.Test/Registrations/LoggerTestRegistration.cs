namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using Microsoft.Extensions.Logging;

    internal class LoggerTestRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                var logger = loggerFactory.CreateLogger(nameof(LoggerTestRegistration));
                container.RegisterInstance<ILogger>(logger);
            }
        }
    }
}