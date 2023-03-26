namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Builder;
    using CompositionRoot;
    using CompositionRoot.Registration;
    using Contract;
    using Endpoint;
    using GenericHost;
    using IntegrationTransport.Api.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using Registrations;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        private const string EndpointDuplicatesWasFound = "Endpoint duplicates was found: {0}. Horizontal scaling in the same process doesn't make sense.";
        private const string RequireUseTransportCall = ".UseIntegrationTransport() should be called before any endpoint declarations. {0}";
        private const string RequireTransportInjection = "Unable to find IIntegrationTransport injection.";
        private const string RequireUseEndpointCall = ".UseEndpoint() with identity {0} should be called during host declaration";

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(this IHost host, EndpointIdentity endpointIdentity)
        {
            return host
                       .Services
                       .GetServices<IDependencyContainer>()
                       .SingleOrDefault(IsEndpointContainer(endpointIdentity))
                   ?? throw new InvalidOperationException(RequireUseEndpointCall.Format(endpointIdentity));

            static Func<IDependencyContainer, bool> IsEndpointContainer(EndpointIdentity endpointIdentity)
            {
                return container => ExecutionExtensions
                    .Try(state => IsEndpointContainerUnsafe(state, endpointIdentity), container)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsEndpointContainerUnsafe(IDependencyContainer dependencyContainer, EndpointIdentity endpointIdentity)
            {
                return dependencyContainer
                   .Resolve<EndpointIdentity>()
                   .Equals(endpointIdentity);
            }
        }

        /// <summary>
        /// Gets endpoint dependency container
        /// </summary>
        /// <param name="host">IHost</param>
        /// <param name="logicalName">Logical name</param>
        /// <returns>IDependencyContainer</returns>
        public static IDependencyContainer GetEndpointDependencyContainer(this IHost host, string logicalName)
        {
            return host
                       .Services
                       .GetServices<IDependencyContainer>()
                       .SingleOrDefault(IsEndpointContainer(logicalName))
                   ?? throw new InvalidOperationException(RequireUseEndpointCall.Format(logicalName));

            static Func<IDependencyContainer, bool> IsEndpointContainer(string logicalName)
            {
                return container => ExecutionExtensions
                    .Try(state => IsEndpointContainerUnsafe(state, logicalName), container)
                    .Catch<Exception>()
                    .Invoke(_ => false);
            }

            static bool IsEndpointContainerUnsafe(IDependencyContainer dependencyContainer, string logicalName)
            {
                return dependencyContainer
                   .Resolve<EndpointIdentity>()
                   .LogicalName
                   .Equals(logicalName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="assembly">Endpoint assembly (composition root or executable)</param>
        /// <param name="optionsFactory">Endpoint options factory</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseEndpoint(
            this IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity,
            Assembly assembly,
            Func<HostBuilderContext, IEndpointBuilder, EndpointOptions> optionsFactory)
        {
            hostBuilder.CheckDuplicates(endpointIdentity);

            return hostBuilder.ConfigureServices((context, serviceCollection) =>
            {
                var builder = ConfigureBuilder(hostBuilder, endpointIdentity, assembly);

                var options = optionsFactory(context, builder);

                var dependencyContainer = BuildDependencyContainer(options);

                serviceCollection.AddSingleton<IDependencyContainer>(dependencyContainer);
            });
        }

        internal static void CheckDuplicates(this IHostBuilder hostBuilder, EndpointIdentity endpointIdentity)
        {
            if (!hostBuilder.Properties.TryGetValue(nameof(EndpointIdentity), out var value)
                || value is not Dictionary<string, EndpointIdentity> endpointIdentities)
            {
                endpointIdentities = new Dictionary<string, EndpointIdentity>(StringComparer.OrdinalIgnoreCase);
                hostBuilder.Properties[nameof(EndpointIdentity)] = endpointIdentities;
            }

            if (!endpointIdentities.TryAdd(endpointIdentity.LogicalName, endpointIdentity))
            {
                throw new InvalidOperationException(EndpointDuplicatesWasFound.Format(endpointIdentity.LogicalName));
            }
        }

        private static IDependencyContainer BuildDependencyContainer(EndpointOptions options)
        {
            return DependencyContainer.CreateBoundedAbove(
                options.ContainerOptions,
                options.AboveAssemblies.ToArray());
        }

        private static IEndpointBuilder ConfigureBuilder(
            IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity,
            Assembly assembly)
        {
            var crossCuttingConcernsAssembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(CrossCuttingConcerns)));

            var integrationTransportInjection = hostBuilder.GetIntegrationTransportInjection();
            var settingsDirectoryProvider = hostBuilder.GetSettingsDirectoryProvider();
            var frameworkDependenciesProvider = hostBuilder.GetFrameworkDependenciesProvider();
            var telemetry = SetupTelemetry(endpointIdentity, assembly);

            return new EndpointBuilder(endpointIdentity)
               .WithEndpointPluginAssemblies(crossCuttingConcernsAssembly)
               .ModifyContainerOptions(options => options
                   .WithManualRegistrations(
                       integrationTransportInjection,
                       new GenericEndpointIdentityManualRegistration(endpointIdentity),
                       new SettingsProviderManualRegistration(settingsDirectoryProvider),
                       new LoggerFactoryManualRegistration(endpointIdentity, frameworkDependenciesProvider),
                       new TelemetryManualRegistration(telemetry),
                       new HostStartupActionsRegistryManualRegistration(frameworkDependenciesProvider),
                       new GenericEndpointHostStartupActionManualRegistration())
                   .WithManualVerification(true));
        }

        private static IManualRegistration GetIntegrationTransportInjection(this IHostBuilder hostBuilder)
        {
            if (hostBuilder.Properties.TryGetValue(nameof(IIntegrationTransport), out var value)
                && value is IManualRegistration injection)
            {
                return injection;
            }

            throw new InvalidOperationException(RequireUseTransportCall.Format(RequireTransportInjection));
        }

        private static ITelemetry SetupTelemetry(
            EndpointIdentity endpointIdentity,
            Assembly assembly)
        {
            var serviceName = endpointIdentity.LogicalName;

            var version = assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
                          ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                          ?? assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                          ?? "1.0.0.0";

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(
                    serviceVersion: version,
                    serviceName: endpointIdentity.LogicalName,
                    serviceInstanceId: endpointIdentity.InstanceName,
                    autoGenerateServiceInstanceId: false);

            var tracerProvider = OpenTelemetry.Sdk
                .CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(serviceName)
                .AddOtlpExporter()
                .Build();

            var meterProvider = OpenTelemetry.Sdk
                .CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(serviceName)
                .AddOtlpExporter()
                .Build();

            return new Telemetry(
                tracerProvider!,
                tracerProvider.GetTracer(serviceName, version),
                meterProvider!,
                new Meter(serviceName, version));
        }
    }
}