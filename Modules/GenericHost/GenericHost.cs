namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoRegistration.Extensions;
    using AutoWiringApi.Enumerations;
    using Basics;
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
        /// Use in-memory integration transport implementation
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transportOptions">In-memory integration transport options</param>
        /// <param name="compositeEndpoint">Composite endpoint</param>
        /// <returns>InMemoryIntegrationTransport</returns>
        public static IHostBuilder UseInMemoryTransport(
            this IHostBuilder hostBuilder,
            InMemoryIntegrationTransportOptions transportOptions,
            ICompositeEndpoint compositeEndpoint)
        {
            return hostBuilder.UseInMemoryTransport(transportOptions, compositeEndpoint.Endpoints.ToArray());
        }

        /// <summary>
        /// Use in-memory integration transport implementation
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transportOptions">In-memory integration transport options</param>
        /// <param name="genericEndpoints">Endpoint instances</param>
        /// <returns>InMemoryIntegrationTransport</returns>
        public static IHostBuilder UseInMemoryTransport(
            this IHostBuilder hostBuilder,
            InMemoryIntegrationTransportOptions transportOptions,
            params IGenericEndpoint[] genericEndpoints)
        {
            var transport = InMemoryIntegrationTransport(transportOptions);
            return hostBuilder.UseTransport(transport, genericEndpoints);
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
        /// Use transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="compositeEndpoint">Composite endpoint</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseTransport(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            ICompositeEndpoint compositeEndpoint)
        {
            return UseTransport(hostBuilder, transport, compositeEndpoint.Endpoints.ToArray());
        }

        /// <summary>
        /// Use transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="genericEndpoints">Endpoint instances</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseTransport(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            params IGenericEndpoint[] genericEndpoints)
        {
            ValidateEndpoints(genericEndpoints);

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
                    genericEndpoints);
            }
        }

        private static void ValidateEndpoints(IGenericEndpoint[] genericEndpoints)
        {
            var duplicates = genericEndpoints
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
                throw new InvalidOperationException($"`{nameof(UseTransport)}` can only be used once on the same host instance because subsequent calls would introduce errors.");
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