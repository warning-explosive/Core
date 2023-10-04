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

        public LoggerFactoryManualRegistration(TransportIdentity transportIdentity)
        {
            _transportIdentity = transportIdentity;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(
                () => container
                    .Advanced
                    .DependencyContainer
                    .Resolve<IFrameworkDependenciesProvider>()
                    .GetRequiredService<ILoggerFactory>(),
                EnLifestyle.Singleton);

            container.Advanced.RegisterDelegate(
                () => container
                    .Advanced
                    .DependencyContainer
                    .Resolve<IFrameworkDependenciesProvider>()
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(_transportIdentity.ToString()),
                EnLifestyle.Singleton);
        }
    }
}