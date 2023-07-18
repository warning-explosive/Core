namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using CompositionRoot;
    using CompositionRoot.Registration;
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
            ((DependencyContainer)container.Advanced.DependencyContainer).SuppressResolveWarnings();

            var simpleInjector = ((DependencyContainer)container.Advanced.DependencyContainer).Container;

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
    }
}