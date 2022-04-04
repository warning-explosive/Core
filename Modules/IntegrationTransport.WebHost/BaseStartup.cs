namespace SpaceEngineers.Core.IntegrationTransport.WebHost
{
    using System;
    using CompositionRoot;
    using Host;
    using Host.Builder;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SimpleInjector;

    /// <summary>
    /// BaseStartup
    /// </summary>
    public abstract class BaseStartup : IStartup
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly Func<ITransportEndpointBuilder, TransportEndpointOptions> _optionsFactory;
        private readonly IConfiguration _configuration;

        /// <summary> .cctor </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="configuration">IConfiguration</param>
        /// <param name="optionsFactory">Transport endpoint options factory</param>
        protected BaseStartup(
            IHostBuilder hostBuilder,
            IConfiguration configuration,
            Func<ITransportEndpointBuilder, TransportEndpointOptions> optionsFactory)
        {
            _hostBuilder = hostBuilder;
            _configuration = configuration;
            _optionsFactory = optionsFactory;
        }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            ConfigureAspNetCoreServices(serviceCollection, _configuration);

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
        public void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var transportDependencyContainer = applicationBuilder
                .ApplicationServices
                .GetTransportEndpointDependencyContainer();

            var simpleInjector = ((DependencyContainer)transportDependencyContainer).Container;

            applicationBuilder
                .ApplicationServices
                .UseSimpleInjector(simpleInjector);

            ConfigureAspNetCoreRequestPipeline(
                applicationBuilder,
                environment,
                configuration);

            GenericHost.HostExtensions.VerifyDependencyContainers(applicationBuilder.ApplicationServices);
        }

        /// <summary>
        /// Configures ASP .NET Core DI container
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection</param>
        /// <param name="configuration">IConfiguration</param>
        protected abstract void ConfigureAspNetCoreServices(
            IServiceCollection serviceCollection,
            IConfiguration configuration);

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