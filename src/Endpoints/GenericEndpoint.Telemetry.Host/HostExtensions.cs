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
    using Microsoft.Extensions.Logging;
    using OpenTelemetry.Logs;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    /// <summary>
    /// HostExtensions
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Configures OpenTelemetry host-wide
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="configureTracingInstrumentation">Configure tracing instrumentation</param>
        /// <param name="configureMetricsInstrumentation">Configure metrics instrumentation</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseOpenTelemetry(
            this IHostBuilder hostBuilder,
            Func<TracerProviderBuilder, TracerProviderBuilder>? configureTracingInstrumentation = default,
            Func<MeterProviderBuilder, MeterProviderBuilder>? configureMetricsInstrumentation = default)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseOpenTelemetry));

            var resourceBuilder = ResourceBuilder.CreateDefault();

            var endpointIdentities = hostBuilder.GetEndpointIdentities();

            foreach (var endpointIdentity in endpointIdentities)
            {
                resourceBuilder.AddService(
                    serviceVersion: endpointIdentity.Version,
                    serviceName: endpointIdentity.LogicalName,
                    serviceInstanceId: endpointIdentity.InstanceName,
                    autoGenerateServiceInstanceId: false);
            }

            var tracerProviderBuilder = OpenTelemetry.Sdk
                .CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder);

            foreach (var endpointIdentity in endpointIdentities)
            {
                tracerProviderBuilder.AddSource(endpointIdentity.LogicalName);
            }

            if (configureTracingInstrumentation != null)
            {
                tracerProviderBuilder = configureTracingInstrumentation(tracerProviderBuilder);
            }

            var tracerProvider = tracerProviderBuilder
                .AddOtlpExporter()
                .Build() !;

            var meterProviderBuilder = OpenTelemetry.Sdk
                .CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder);

            foreach (var endpointIdentity in endpointIdentities)
            {
                meterProviderBuilder.AddMeter(endpointIdentity.LogicalName);
            }

            if (configureMetricsInstrumentation != null)
            {
                meterProviderBuilder = configureMetricsInstrumentation(meterProviderBuilder);
            }

            var meterProvider = meterProviderBuilder
                .AddOtlpExporter()
                .Build() !;

            return hostBuilder
                .ConfigureLogging(loggingBuilder => loggingBuilder
                    .AddOpenTelemetry(options =>
                    {
                        options.SetResourceBuilder(resourceBuilder);

                        options.IncludeScopes = false;
                        options.IncludeFormattedMessage = true;
                        options.ParseStateValues = false;

                        options.AddOtlpExporter();
                    }))
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton(tracerProvider);
                    serviceCollection.AddSingleton(meterProvider);
                });
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

        private static IReadOnlyCollection<EndpointIdentity> GetEndpointIdentities(this IHostBuilder hostBuilder)
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