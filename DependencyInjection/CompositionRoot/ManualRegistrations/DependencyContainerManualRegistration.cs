namespace SpaceEngineers.Core.CompositionRoot.ManualRegistrations
{
    using Api.Abstractions;
    using Api.Abstractions.Container;
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
                .RegisterInstance(_options.ConstructorResolutionBehavior.GetType(), _options.ConstructorResolutionBehavior)
                .RegisterInstance<DependencyContainer>(_dependencyContainer)
                .RegisterInstance<IDependencyContainer>(_dependencyContainer)
                .RegisterInstance<IScopedContainer>(_dependencyContainer)
                .RegisterInstance<IDependencyContainerImplementation>(_dependencyContainer.Container)
                .RegisterInstance(_dependencyContainer.Container.GetType(), _dependencyContainer.Container);
        }
    }
}