namespace SpaceEngineers.Core.AutoRegistration.ManualRegistrations
{
    using Abstractions;

    internal class DependencyContainerManualRegistration : IManualRegistration
    {
        private readonly DependencyContainer _dependencyContainer;

        internal DependencyContainerManualRegistration(DependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<DependencyContainer>(_dependencyContainer)
                .RegisterInstance<IDependencyContainer>(_dependencyContainer)
                .RegisterInstance<IScopedContainer>(_dependencyContainer);
        }
    }
}