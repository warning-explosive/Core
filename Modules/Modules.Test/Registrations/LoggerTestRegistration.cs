namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Enumerations;
    using Microsoft.Extensions.Logging;

    internal class LoggerTestRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterFactory<ILogger>(() =>
                {
                    using var loggerFactory = new LoggerFactory();
                    return loggerFactory.CreateLogger(nameof(LoggerTestRegistration));
                },
                EnLifestyle.Singleton);
        }
    }
}