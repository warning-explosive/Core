namespace SpaceEngineers.Core.DataAccess.Orm.Host.Registrations
{
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions.Registration;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

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