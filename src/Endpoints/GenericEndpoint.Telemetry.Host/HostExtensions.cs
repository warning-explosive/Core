namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Contract;
    using GenericEndpoint.Host.Builder;
    using GenericHost;
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
        /// <summary>
        /// Configures OpenTelemetry host-wide
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseOpenTelemetry(this IHostBuilder hostBuilder)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseOpenTelemetry));

            var tracerProviders = new Dictionary<EndpointIdentity, TracerProvider>();
            var meterProviders = new Dictionary<EndpointIdentity, MeterProvider>();

            foreach (var endpointIdentity in hostBuilder.GetEndpointIdentities())
            {
                var resourceBuilder = GetResourceBuilder(endpointIdentity);
                tracerProviders.Add(endpointIdentity, GetTracerProvider(endpointIdentity, resourceBuilder));
                meterProviders.Add(endpointIdentity, GetMeterProvider(endpointIdentity, resourceBuilder));
            }

            var telemetryProvider = new TelemetryProvider(tracerProviders, meterProviders);

            return hostBuilder
                /*.ConfigureLogging(loggingBuilder => loggingBuilder
                    .AddOpenTelemetry(options =>
                    {
                        // TODO: #200 - setup open telemetry logging
                        options.SetResourceBuilder(ResourceBuilder.CreateDefault());

                        options.IncludeScopes = false;
                        options.IncludeFormattedMessage = true;
                        options.ParseStateValues = false;

                        options.AddOtlpExporter(_ => { });
                    }))*/
                .ConfigureServices(serviceCollection => serviceCollection.AddSingleton<ITelemetryProvider>(telemetryProvider));

            static ResourceBuilder GetResourceBuilder(EndpointIdentity endpointIdentity)
            {
                return ResourceBuilder
                    .CreateDefault()
                    .AddService(
                        serviceVersion: endpointIdentity.Version,
                        serviceName: endpointIdentity.LogicalName,
                        serviceInstanceId: endpointIdentity.InstanceName,
                        autoGenerateServiceInstanceId: false);
            }

            static TracerProvider GetTracerProvider(
                EndpointIdentity endpointIdentity,
                ResourceBuilder resourceBuilder)
            {
                // TODO: #200 - handle exported duplicates
                return OpenTelemetry.Sdk
                    .CreateTracerProviderBuilder()
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
                    .Build() !;
            }

            static MeterProvider GetMeterProvider(
                EndpointIdentity endpointIdentity,
                ResourceBuilder resourceBuilder)
            {
                return OpenTelemetry.Sdk
                    .CreateMeterProviderBuilder()
                    .SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
                    .Build() !;
            }
        }

        /// <summary>
        /// With OpenTelemetry
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <returns>Configured IEndpointBuilder</returns>
        public static IEndpointBuilder WithOpenTelemetry(
            this IEndpointBuilder builder)
        {
            builder.CheckMultipleCalls(nameof(WithOpenTelemetry));

            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(GenericEndpoint),
                    nameof(Telemetry)));

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assembly)
                    .WithManualRegistrations(new TelemetryManualRegistration()));
        }

        private static IEnumerable<EndpointIdentity> GetEndpointIdentities(this IHostBuilder hostBuilder)
        {
            if (!hostBuilder.TryGetPropertyValue<Dictionary<string, EndpointIdentity>>(nameof(EndpointIdentity), out var endpoints)
                || !endpoints.Any())
            {
                throw new InvalidOperationException($".{nameof(UseOpenTelemetry)}() should be called after all endpoint declarations");
            }

            return endpoints.Values;
        }
    }
}