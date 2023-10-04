namespace SpaceEngineers.Core.GenericEndpoint.Web.Api.Host
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Registrations;
    using SimpleInjector;
    using Basics;
    using CompositionRoot;
    using SpaceEngineers.Core.GenericEndpoint.Authorization.Host;
    using Contract;
    using SpaceEngineers.Core.GenericEndpoint.Host;
    using SpaceEngineers.Core.GenericEndpoint.Host.Builder;

    /// <summary>
    /// BaseStartup
    /// </summary>
    public abstract class BaseStartup : IStartup
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly IConfiguration _configuration;
        private readonly Func<IEndpointBuilder, EndpointOptions> _optionsFactory;

        /// <summary> .cctor </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="configuration">IConfiguration</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="optionsFactory">EndpointOptions factory</param>
        protected BaseStartup(
            IHostBuilder hostBuilder,
            IConfiguration configuration,
            EndpointIdentity endpointIdentity,
            Func<IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            _hostBuilder = hostBuilder;
            _configuration = configuration;
            EndpointIdentity = endpointIdentity;
            _optionsFactory = optionsFactory;
        }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        protected EndpointIdentity EndpointIdentity { get; }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            ConfigureAspNetCoreServices(serviceCollection, _configuration);

            _hostBuilder.UseEndpoint(
                EndpointIdentity,
                (_, builder) => _optionsFactory(
                    WithWebAuthorization(
                        WithSimpleInjector(builder, serviceCollection),
                        _hostBuilder,
                        _configuration)));

            static IEndpointBuilder WithSimpleInjector(
                IEndpointBuilder builder,
                IServiceCollection serviceCollection)
            {
                return builder
                    .ModifyContainerOptions(options => options
                        .WithManualRegistrations(new SimpleInjectorIntegrationManualRegistration(serviceCollection)));
            }

            static IEndpointBuilder WithWebAuthorization(
                IEndpointBuilder builder,
                IHostBuilder hostBuilder,
                IConfiguration configuration)
            {
                // TODO: #225 review
                /*var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();*/

                var assemblies = new[]
                {
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(SpaceEngineers.Core.Web), nameof(SpaceEngineers.Core.Web.Api))),
                    AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(GenericEndpoint), nameof(Authorization), nameof(Authorization.Web)))
                };

                return builder
                    .WithAuthorization(configuration)
                    .ModifyContainerOptions(options => options
                        .WithPluginAssemblies(assemblies)
                        .WithManualRegistrations(
                            /*new HttpContextAccessorManualRegistration(frameworkDependenciesProvider)*/));
            }
        }

        /// <inheritdoc />
        public void Configure(
            IApplicationBuilder applicationBuilder,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            var dependencyContainer = applicationBuilder
                .ApplicationServices
                .GetEndpointDependencyContainer(EndpointIdentity);
            
            var simpleInjector = ((DependencyContainer)dependencyContainer).Container;

            applicationBuilder
                .ApplicationServices
                .UseSimpleInjector(simpleInjector);

            ConfigureAspNetCoreRequestPipeline(
                applicationBuilder,
                environment,
                configuration);
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