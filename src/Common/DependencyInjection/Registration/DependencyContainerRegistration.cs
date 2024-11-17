namespace SpaceEngineers.Core.DependencyInjection.Registration;

internal class DependencyContainerRegistration : IDependencyInjectionRegistration
{
    private readonly DependencyContainer _dependencyContainer;
    private readonly DependencyContainerOptions _options;

    internal DependencyContainerRegistration(
        DependencyContainer dependencyContainer,
        DependencyContainerOptions options)
    {
        _dependencyContainer = dependencyContainer;
        _options = options;
    }

    public void Register(IDependencyInjectionRegistrationContainer container)
    {
        container
            .RegisterInstance<IDependencyContainer>(_dependencyContainer)
            .RegisterInstance<DependencyContainer>(_dependencyContainer)
            .RegisterInstance<DependencyContainerOptions>(_options);
    }
}