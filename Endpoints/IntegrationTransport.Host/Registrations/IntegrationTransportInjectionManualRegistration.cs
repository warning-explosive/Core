namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using Api.Abstractions;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using GenericEndpoint.RpcRequest;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    internal class IntegrationTransportInjectionManualRegistration : IManualRegistration
    {
        private readonly IDependencyContainer _dependencyContainer;

        public IntegrationTransportInjectionManualRegistration(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate<IExecutableIntegrationTransport>(() => _dependencyContainer.Resolve<IExecutableIntegrationTransport>(), EnLifestyle.Singleton);
            container.Advanced.RegisterDelegate<IConfigurableIntegrationTransport>(() => _dependencyContainer.Resolve<IConfigurableIntegrationTransport>(), EnLifestyle.Singleton);
            container.Advanced.RegisterDelegate<IIntegrationTransport>(() => _dependencyContainer.Resolve<IIntegrationTransport>(), EnLifestyle.Singleton);
            container.Advanced.RegisterDelegate<IRpcRequestRegistry>(() => _dependencyContainer.Resolve<IRpcRequestRegistry>(), EnLifestyle.Singleton);
        }
    }
}