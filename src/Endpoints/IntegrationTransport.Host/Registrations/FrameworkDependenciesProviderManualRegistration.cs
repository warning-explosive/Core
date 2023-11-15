namespace SpaceEngineers.Core.IntegrationTransport.Host.Registrations
{
    using CompositionRoot.Registration;
    using GenericHost;

    internal class FrameworkDependenciesProviderManualRegistration : IManualRegistration
    {
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public FrameworkDependenciesProviderManualRegistration(IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_frameworkDependenciesProvider);
        }
    }
}