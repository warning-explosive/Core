namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using System;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Host;
    using Host.Builder;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SimpleInjector;

    /// <summary>
    /// SimpleInjectorBaseStartup
    /// </summary>
    public abstract class SimpleInjectorBaseStartup : BaseStartup
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly Func<ITransportEndpointBuilder, TransportEndpointOptions> _optionsFactory;

        /// <summary> .cctor </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="configuration">IConfiguration</param>
        /// <param name="optionsFactory">Transport endpoint options factory</param>
        protected SimpleInjectorBaseStartup(
            IHostBuilder hostBuilder,
            IConfiguration configuration,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
            : base(configuration)
        {
            _hostBuilder = hostBuilder;
            _optionsFactory = optionsFactory;
        }

        /// <inheritdoc />
        public sealed override void ConfigureServices(IServiceCollection serviceCollection)
        {
            ConfigureAspNetCoreServices(serviceCollection);

            HostExtensions.InitializeIntegrationTransport(_hostBuilder, WithSimpleInjector(_optionsFactory, serviceCollection))(serviceCollection);

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
        public sealed override void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var transportDependencyContainer = applicationBuilder
                .ApplicationServices
                .GetTransportEndpointDependencyContainer();

            var simpleInjector = ExtractSimpleInjector(transportDependencyContainer);

            applicationBuilder
                .ApplicationServices
                .UseSimpleInjector(simpleInjector);

            ConfigureAspNetCoreRequestPipeline(
                applicationBuilder,
                environment,
                configuration);

            GenericHost.HostExtensions.VerifyDependencyContainers(applicationBuilder.ApplicationServices);

            static Container ExtractSimpleInjector(
                IDependencyContainer dependencyContainer)
            {
                return dependencyContainer
                    .GetFieldValue<Container?>("_container")
                    .EnsureNotNull("Dependency container should have SimpleInjector container reference inside");
            }
        }

        /// <summary>
        /// Configures ASP .NET Core DI container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        protected abstract void ConfigureAspNetCoreServices(
            IServiceCollection serviceCollection);

        /// <summary>
        /// Configures ASP .NET Core request pipeline
        /// </summary>
        /// <param name="applicationBuilder">IApplicationBuilder</param>
        /// <param name="environment">IWebHostEnvironment</param>
        /// <param name="configuration">IConfiguration</param>
        protected abstract void ConfigureAspNetCoreRequestPipeline(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration);
    }
}