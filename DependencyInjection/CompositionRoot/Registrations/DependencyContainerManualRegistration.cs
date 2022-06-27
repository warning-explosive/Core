namespace SpaceEngineers.Core.CompositionRoot.Registrations
{
    using Api.Abstractions;
    using Api.Abstractions.Registration;

    internal class DependencyContainerManualRegistration : IManualRegistration
    {
        private readonly DependencyContainer _dependencyContainer;
        private readonly DependencyContainerOptions _options;

        internal DependencyContainerManualRegistration(
            DependencyContainer dependencyContainer,
            DependencyContainerOptions options)
        {
            _dependencyContainer = dependencyContainer;
            _options = options;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
               .RegisterInstance<DependencyContainerOptions>(_options)
               .RegisterInstance<IConstructorResolutionBehavior>(_options.ConstructorResolutionBehavior)
               .RegisterInstance<IDependencyContainer>(_dependencyContainer);
        }
    }
}