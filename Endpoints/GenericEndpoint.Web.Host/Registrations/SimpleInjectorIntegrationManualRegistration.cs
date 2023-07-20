namespace SpaceEngineers.Core.GenericEndpoint.Web.Host.Registrations
{
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;
    using CompositionRoot;
    using CompositionRoot.Registration;

    internal class SimpleInjectorIntegrationManualRegistration : IManualRegistration
    {
        private readonly IServiceCollection _serviceCollection;

        public SimpleInjectorIntegrationManualRegistration(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void Register(IManualRegistrationsContainer container)
        {
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