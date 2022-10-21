namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Registration;
    using GenericHost.Api.Abstractions;

    internal class HostStartupActionsRegistryManualRegistration : IManualRegistration
    {
        private readonly IFrameworkDependenciesProvider _frameworkDependenciesProvider;

        public HostStartupActionsRegistryManualRegistration(IFrameworkDependenciesProvider frameworkDependenciesProvider)
        {
            _frameworkDependenciesProvider = frameworkDependenciesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.Advanced.RegisterDelegate(
                () => _frameworkDependenciesProvider.GetRequiredService<IHostStartupActionsRegistry>(),
                EnLifestyle.Singleton);
        }
    }
}