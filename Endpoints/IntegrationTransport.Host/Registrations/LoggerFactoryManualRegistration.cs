namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using Api;
    using CompositionRoot.Registration;
    using GenericHost;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class LoggerFactoryManualRegistration : IManualRegistration
    {
        private readonly TransportIdentity _transportIdentity;
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public LoggerFactoryManualRegistration(
            TransportIdentity transportIdentity,
            IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _transportIdentity = transportIdentity;
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(
                () => _frameworkDependenciesProvider
                    .GetRequiredService<ILoggerFactory>(),
                EnLifestyle.Singleton);

            container.Advanced.RegisterDelegate(
                () => _frameworkDependenciesProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(_transportIdentity.ToString()),
                EnLifestyle.Singleton);
        }
    }
}