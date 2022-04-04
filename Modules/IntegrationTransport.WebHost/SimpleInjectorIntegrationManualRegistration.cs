namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;

    internal class SimpleInjectorIntegrationManualRegistration : IManualRegistration
    {
        private readonly IServiceCollection _serviceCollection;

        public SimpleInjectorIntegrationManualRegistration(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            ((DependencyContainer)container.Advanced.Container).SuppressResolveWarnings();

            var simpleInjector = ExtractSimpleInjector(container.Advanced.Container);

            _serviceCollection.AddSimpleInjector(
                simpleInjector,
                options =>
                {
                    options.EnableHostedServiceResolution = true;
                    options.AutoCrossWireFrameworkComponents = true;
                    options.DisposeContainerWithServiceProvider = false;

                    options
                        .AddAspNetCore(ServiceScopeReuseBehavior.OnePerNestedScope)
                        .AddControllerActivation(Lifestyle.Transient);
                });
        }

        private static Container ExtractSimpleInjector(
            IDependencyContainer dependencyContainerImplementation)
        {
            return dependencyContainerImplementation
                .GetFieldValue<Container?>("_container")
                .EnsureNotNull("Dependency container should have SimpleInjector container reference inside");
        }
    }
}