namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoRegistration.Extensions;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using SimpleInjector;
    using Transport;

    /// <summary>
    /// Generic host entry point
    /// </summary>
    public static class GenericHost
    {
        private const string HostAlreadyConfigured = nameof(HostAlreadyConfigured);

        /// <summary>
        /// Generates manual registration for generic endpoint
        /// </summary>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <returns>Generic endpoint manual registration</returns>
        public static IManualRegistration GenericEndpointRegistration(EndpointIdentity endpointIdentity)
        {
            return new Endpoint.GenericEndpointManualRegistration(endpointIdentity);
        }

        /// <summary>
        /// Builds in-memory integration transport implementation
        /// </summary>
        /// <param name="transportOptions">In-memory integration transport options</param>
        /// <returns>IIntegrationTransport instance</returns>
        public static IIntegrationTransport InMemoryIntegrationTransport(InMemoryIntegrationTransportOptions transportOptions)
        {
            var registration = InMemoryIntegrationTransportRegistration(transportOptions);

            var registrations = transportOptions
                .AdditionalRegistrations
                .Concat(new[] { registration })
                .ToArray();

            var containerOptions = new DependencyContainerOptions
            {
                ManualRegistrations = registrations
            };

            return DependencyContainer
                .CreateExactlyBounded(Array.Empty<Assembly>(), containerOptions)
                .Resolve<InMemoryIntegrationTransport>();
        }

        /// <summary>
        /// Configure host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="endpointOptions">Endpoint options</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder ConfigureHost(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            params EndpointOptions[] endpointOptions)
        {
            ValidateEndpoints(endpointOptions);

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                ValidateConfiguration(ctx);

                serviceCollection.AddSingleton(_ => transport);
                serviceCollection.AddHostedService(BuildHostedService);
            });

            TransportHostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new TransportHostedService(
                    serviceProvider.GetRequiredService<ILogger<TransportHostedService>>(),
                    serviceProvider.GetRequiredService<IIntegrationTransport>(),
                    endpointOptions);
            }
        }

        private static void ValidateEndpoints(EndpointOptions[] endpointOptions)
        {
            var duplicates = endpointOptions
                .GroupBy(e => e.Identity)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.ToString())
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException($"Endpoint duplicates found: {string.Join(", ", duplicates)}");
            }
        }

        private static void ValidateConfiguration(HostBuilderContext ctx)
        {
            if (ctx.Properties.ContainsKey(HostAlreadyConfigured))
            {
                throw new InvalidOperationException($"`{nameof(ConfigureHost)}` can only be used once on the same host instance because subsequent calls would introduce errors.");
            }

            ctx.Properties[HostAlreadyConfigured] = null;
        }

        private static IManualRegistration InMemoryIntegrationTransportRegistration(InMemoryIntegrationTransportOptions transportOptions)
        {
            Type[] headerProviders;

            using (var templateContainer = new Container())
            {
                headerProviders = typeof(IMessageHeaderProvider)
                    .GetTypesToRegister(templateContainer, AssembliesExtensions.AllFromCurrentDomain())
                    .ToArray();
            }

            return DependencyContainerOptions
                .DelegateRegistration(container =>
                {
                    container.Register<IIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);
                    container.Register<InMemoryIntegrationTransport, InMemoryIntegrationTransport>(EnLifestyle.Singleton);

                    container.Register<IUbiquitousIntegrationContext, InMemoryUbiquitousIntegrationContext>(EnLifestyle.Singleton);
                    container.Register<InMemoryUbiquitousIntegrationContext, InMemoryUbiquitousIntegrationContext>(EnLifestyle.Singleton);

                    var behavior = transportOptions.EndpointInstanceSelectionBehavior;
                    container.RegisterInstance(behavior);
                    container.RegisterInstance(behavior.GetType(), behavior);

                    var assemblyName = AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(SpaceEngineers.Core.GenericEndpoint));
                    var typeFullName = AssembliesExtensions.BuildName(assemblyName, "Internals", "IntegrationMessageFactory");
                    var messageFactoryType = AssembliesExtensions.FindRequiredType(assemblyName, typeFullName);
                    var messageFactoryLifestyle = messageFactoryType.Lifestyle();
                    container.Register(typeof(IIntegrationMessageFactory), messageFactoryType, messageFactoryLifestyle);
                    container.Register(messageFactoryType, messageFactoryType, messageFactoryLifestyle);

                    container.RegisterCollection<IMessageHeaderProvider>(headerProviders, messageFactoryLifestyle);
                });
        }
    }
}