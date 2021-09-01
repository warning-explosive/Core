namespace SpaceEngineers.Core.Modules.Test.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    internal class ModulesTestLoggerFactoryManualRegistration : IManualRegistration
    {
        public void Register(IManualRegistrationsContainer container)
        {
            var name = AssembliesExtensions.BuildName(nameof(Core), nameof(Core.Modules), nameof(Core.Modules.Test));

            container.Advanced.RegisterFactory<ILogger>(() =>
                {
                    using var loggerFactory = new LoggerFactory();
                    return loggerFactory.CreateLogger(name);
                },
                EnLifestyle.Singleton);
        }
    }
}