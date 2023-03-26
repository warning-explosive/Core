namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using Contract;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    internal class LoggerFactoryManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public LoggerFactoryManualRegistration(
            EndpointIdentity endpointIdentity,
            IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _endpointIdentity = endpointIdentity;
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate<ILoggerFactory>(
                () => _frameworkDependenciesProvider
                    .GetRequiredService<ILoggerFactory>(),
                EnLifestyle.Singleton);

            container.Advanced.RegisterDelegate<ILogger>(
                () => _frameworkDependenciesProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(_endpointIdentity.ToString()),
                EnLifestyle.Singleton);
        }
    }
}