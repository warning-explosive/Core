namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using Abstractions;
    using AutoWiring.Api.Services;

    internal class DependencyContainerManualRegistration : IManualRegistration
    {
        private readonly DependencyContainer _dependencyContainer;
        private readonly ITypeProvider _typeProvider;
        private readonly IAutoWiringServicesProvider _servicesProvider;

        internal DependencyContainerManualRegistration(
            DependencyContainer dependencyContainer,
            ITypeProvider typeProvider,
            IAutoWiringServicesProvider servicesProvider)
        {
            _dependencyContainer = dependencyContainer;
            _typeProvider = typeProvider;
            _servicesProvider = servicesProvider;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container
                .RegisterInstance<DependencyContainer>(_dependencyContainer)
                .RegisterInstance<IDependencyContainer>(_dependencyContainer)
                .RegisterInstance<IScopedContainer>(_dependencyContainer)

                .RegisterInstance(_typeProvider.GetType(), _typeProvider)
                .RegisterInstance<ITypeProvider>(_typeProvider)

                .RegisterInstance(_servicesProvider.GetType(), _servicesProvider)
                .RegisterInstance<IAutoWiringServicesProvider>(_servicesProvider);
        }
    }
}