namespace SpaceEngineers.Core.IntegrationTransport.WebHost.SimpleInjector
{
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
            _serviceCollection.AddSimpleInjector(
                SimpleInjectorBaseStartup.ExtractSimpleInjector(container.Advanced.Container),
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
    }
}