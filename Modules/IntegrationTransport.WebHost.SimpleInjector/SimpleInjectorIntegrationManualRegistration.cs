namespace SpaceEngineers.Core.IntegrationTransport.WebHost.SimpleInjector
{
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Abstractions.Registration;
    using global::SimpleInjector;
    using Microsoft.Extensions.DependencyInjection;

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
                    options.EnableHostedServiceResolution = false;
                    options.AutoCrossWireFrameworkComponents = false;
                    options.DisposeContainerWithServiceProvider = false;

                    options
                        .AddAspNetCore(ServiceScopeReuseBehavior.OnePerNestedScope)
                        .AddControllerActivation(Lifestyle.Scoped);
                });
        }

        private static Container ExtractSimpleInjector(
            IDependencyContainer dependencyContainerImplementation)
        {
            return dependencyContainerImplementation
                .GetPropertyValue<IDependencyContainerImplementation?>("Container")
                .EnsureNotNull($"{nameof(IDependencyContainer)} should have container implementation reference inside")
                .GetFieldValue<Container?>("_container")
                .EnsureNotNull($"{nameof(IDependencyContainerImplementation)} should have SimpleInjector container reference inside");
        }
    }
}