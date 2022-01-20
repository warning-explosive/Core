namespace SpaceEngineers.Core.IntegrationTransport.Host.ManualRegistrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Registration;
    using GenericEndpoint.Contract;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;

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
            container.Advanced.RegisterDelegate<ILogger>(() => _frameworkDependenciesProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(_endpointIdentity.ToString()), EnLifestyle.Singleton);
        }
    }
}