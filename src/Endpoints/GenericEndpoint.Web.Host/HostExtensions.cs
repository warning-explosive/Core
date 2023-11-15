namespace SpaceEngineers.Core.GenericEndpoint.Web.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO.Compression;
    using System.Linq;
    using Auth;
    using Authorization.Host;
    using Basics;
    using CompositionRoot;
    using Contract;
    using Endpoint;
    using GenericEndpoint.Host.Builder;
    using GenericHost;
    using JwtAuthentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Registrations;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore.Mvc;
    using SpaceEngineers.Core.GenericEndpoint.Host;
    using Swagger;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// With web api
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithWebApi(this IEndpointBuilder builder)
        {
            builder.CheckMultipleCalls(nameof(WithWebApi));

            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Web), nameof(Core.Web.Api)))
            };

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assemblies)
                    .WithAdditionalOurTypes(typeof(WebApiConfigurationVerifier))
                    .WithManualRegistrations(
                        new HttpContextAccessorManualRegistration(),
                        new AspNetControllersManualRegistration()));
        }

        /// <summary>
        /// Use web-api gateway endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>Configured IHostBuilder</returns>
        [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
        public static IHostBuilder UseWebApiGateway(
            this IHostBuilder hostBuilder)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseWebApiGateway));

            return hostBuilder
                .ConfigureWebHostDefaults(builder => builder
                    .UseStartup<WebApplicationStartup>()
                    .ConfigureServices((context, serviceCollection) =>
                    {
                        serviceCollection.AddMvcCore(options => options.Filters.Add(new AuthorizeFilter()));

                        var mvcBuilder = serviceCollection.AddControllers();

                        var controllersAssemblies = TypeExtensions
                            .AllTypes()
                            .Where(type => type.IsController())
                            .Select(type => type.Assembly)
                            .Distinct()
                            .ToList();

                        foreach (var assembly in controllersAssemblies)
                        {
                            mvcBuilder.AddApplicationPart(assembly);
                        }

                        serviceCollection.AddHttpContextAccessor();

                        serviceCollection.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());
                        serviceCollection.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

                        serviceCollection.AddAuth(context.Configuration);

                        serviceCollection.AddSwagger(hostBuilder.GetEndpointIdentities());

                        // IStartupFilter
                        serviceCollection.AddSingleton(BuildRequestScopingStartupFilter);

                        // IControllerActivator
                        serviceCollection.Replace(ServiceDescriptor.Transient(BuildControllerActivator));

                        // IWebApiFeaturesProvider
                        serviceCollection.AddSingleton(BuildWebApiFeaturesProvider);

                        // ITokenProvider
                        serviceCollection.AddSingleton<JwtSecurityTokenHandler>();
                        serviceCollection.AddSingleton(context.Configuration.GetJwtAuthenticationConfiguration());
                        serviceCollection.AddSingleton<ITokenProvider, JwtTokenProvider>();
                    }));
        }

        private static IStartupFilter BuildRequestScopingStartupFilter(IServiceProvider serviceProvider)
        {
            var containers = serviceProvider
                .GetServices<GenericEndpointDependencyContainer>()
                .SelectMany(wrapper => MapControllersOnContainers(wrapper.DependencyContainer))
                .ToDictionary(it => it.Key, it => it.Value);

            return new CompositeRequestScopingStartupFilter(
                containers,
                serviceProvider.GetRequiredService<ILogger<CompositeRequestScopingStartupFilter>>());

            static IEnumerable<KeyValuePair<Type, Container>> MapControllersOnContainers(
                IDependencyContainer dependencyContainer)
            {
                var simpleInjector = ((DependencyContainer)dependencyContainer).Container;

                return dependencyContainer
                    .Resolve<ITypeProvider>()
                    .OurTypes
                    .Where(type => type.IsController())
                    .Select(type => new KeyValuePair<Type, Container>(type, simpleInjector));
            }
        }

        private static IControllerActivator BuildControllerActivator(IServiceProvider serviceProvider)
        {
            var activators = serviceProvider
                .GetServices<GenericEndpointDependencyContainer>()
                .SelectMany(wrapper => MapControllersOnActivators(wrapper.DependencyContainer))
                .ToDictionary(it => it.Key, it => it.Value);

            return new CompositeControllerActivator(activators);

            static IEnumerable<KeyValuePair<Type, IControllerActivator>> MapControllersOnActivators(
                IDependencyContainer dependencyContainer)
            {
                var simpleInjector = ((DependencyContainer)dependencyContainer).Container;

                IControllerActivator activator = new SimpleInjectorControllerActivator(simpleInjector);

                return dependencyContainer
                    .Resolve<ITypeProvider>()
                    .OurTypes
                    .Where(type => type.IsController())
                    .Select(type => new KeyValuePair<Type, IControllerActivator>(type, activator));
            }
        }

        private static IWebApiFeaturesProvider BuildWebApiFeaturesProvider(IServiceProvider serviceProvider)
        {
            var controllers = serviceProvider
                .GetServices<GenericEndpointDependencyContainer>()
                .SelectMany(wrapper => wrapper
                    .DependencyContainer
                    .Resolve<ITypeProvider>()
                    .OurTypes
                    .Where(type => type.IsController()))
                .ToArray();

            return new WebApiFeaturesProvider(controllers);
        }

        private static IEnumerable<EndpointIdentity> GetEndpointIdentities(this IHostBuilder hostBuilder)
        {
            if (!hostBuilder.TryGetPropertyValue<Dictionary<string, EndpointIdentity>>(nameof(EndpointIdentity), out var endpoints)
                || !endpoints.Any())
            {
                throw new InvalidOperationException($".{nameof(UseWebApiGateway)}() should be called after all endpoint declarations");
            }

            return endpoints.Values;
        }
    }
}