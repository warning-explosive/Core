namespace SpaceEngineers.Core.IntegrationTransport.WebHost.SimpleInjector
{
    using System;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions.Container;
    using global::SimpleInjector;
    using Host;
    using Host.Builder;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// SimpleInjectorBaseStartup
    /// </summary>
    public abstract class SimpleInjectorBaseStartup : BaseStartup
    {
        private readonly Func<ITransportEndpointBuilder, TransportEndpointOptions> _optionsFactory;

        /// <summary> .cctor </summary>
        /// <param name="optionsFactory">Transport endpoint options factory</param>
        protected SimpleInjectorBaseStartup(Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
        {
            _optionsFactory = optionsFactory;
        }

        /// <inheritdoc />
        protected sealed override void ConfigureApplicationServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvc();

            HostExtensions.InitializeIntegrationTransport(WithSimpleInjector(_optionsFactory, serviceCollection))(serviceCollection);

            static Func<ITransportEndpointBuilder, TransportEndpointOptions> WithSimpleInjector(
                Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory,
                IServiceCollection serviceCollection)
            {
                return builder => optionsFactory(ModifyContainerOptions(builder, serviceCollection));
            }

            static ITransportEndpointBuilder ModifyContainerOptions(
                ITransportEndpointBuilder builder,
                IServiceCollection serviceCollection)
            {
                return builder.ModifyContainerOptions(options => options
                    .WithManualRegistrations(new SimpleInjectorIntegrationManualRegistration(serviceCollection))
                    .WithManualVerification(true));
            }
        }

        /// <inheritdoc />
        protected sealed override void ConfigureApplicationServices(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var dependencyContainer = applicationBuilder
                .ApplicationServices
                .GetTransportEndpointDependencyContainer();

            var simpleInjector = ExtractSimpleInjector(dependencyContainer.Resolve<IDependencyContainerImplementation>());

            applicationBuilder.UseSimpleInjector(simpleInjector);

            ((DependencyContainer)dependencyContainer).Verify();
        }

        private static Container ExtractSimpleInjector(
            IDependencyContainerImplementation dependencyContainerImplementation)
        {
            return dependencyContainerImplementation
                .GetFieldValue<Container?>("_container")
                .EnsureNotNull($"{nameof(IDependencyContainerImplementation)} should have SimpleInjector container reference inside");
        }
    }
}