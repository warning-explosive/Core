namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using Contract;
    using GenericHost;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class LoggerFactoryManualRegistration : IManualRegistration
    {
        private readonly EndpointIdentity _endpointIdentity;

        public LoggerFactoryManualRegistration(EndpointIdentity endpointIdentity)
        {
            _endpointIdentity = endpointIdentity;
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
                    .CreateLogger(_endpointIdentity.ToString()),
                EnLifestyle.Singleton);
        }
    }
}