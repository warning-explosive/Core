namespace SpaceEngineers.Core.DataAccess.Orm.Host.ManualRegistrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;

    internal class LoggerFactoryManualRegistration : IManualRegistration
    {
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public LoggerFactoryManualRegistration(
            IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate<ILogger>(() => _frameworkDependenciesProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Migrations"), EnLifestyle.Singleton);
        }
    }
}