namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Metrics;
    using Basics;
    using Contract;
    using GenericEndpoint.Host.Builder;
    using GenericHost;
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
        /// Use OpenTelemetry logger
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder UseOpenTelemetryLogger(
            this IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity)
        {
            hostBuilder.CheckMultipleCalls(nameof(UseOpenTelemetryLogger));

            // TODO: #225 - verify endpoints and transports
            return hostBuilder
                .ConfigureLogging(loggingBuilder => loggingBuilder
                    .AddOpenTelemetry(options =>
                    {
                        var resourceBuilder = ResourceBuilder
                            .CreateDefault()
                            .AddService(
                                serviceVersion: endpointIdentity.Version,
                                serviceName: endpointIdentity.LogicalName,
                                serviceInstanceId: endpointIdentity.InstanceName,
                                autoGenerateServiceInstanceId: false);

                        options.SetResourceBuilder(resourceBuilder);

                        options.IncludeScopes = false;
                        options.IncludeFormattedMessage = true;
                        options.ParseStateValues = false;

                        options.AddOtlpExporter();
                    }));
        }

        /// <summary>
        /// Use specified endpoint
        /// </summary>
        /// <param name="builder">IEndpointBuilder</param>
        /// <param name="configureTracingInstrumentation">Configure tracing instrumentation</param>
        /// <param name="configureMetricsInstrumentation">Configure metrics instrumentation</param>
        /// <returns>Configured IEndpointBuilder</returns>
        [SuppressMessage("Analysis", "CA2000", Justification = "Meter will be disposed in outer scope by dependency container")]
        public static IEndpointBuilder WithOpenTelemetry(
            this IEndpointBuilder builder,
            Func<TracerProviderBuilder, TracerProviderBuilder>? configureTracingInstrumentation = default,
            Func<MeterProviderBuilder, MeterProviderBuilder>? configureMetricsInstrumentation = default)
        {
            builder.CheckMultipleCalls(nameof(WithOpenTelemetry));

            var endpointIdentity = builder.EndpointIdentity;

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(
                    serviceVersion: endpointIdentity.Version,
                    serviceName: endpointIdentity.LogicalName,
                    serviceInstanceId: endpointIdentity.InstanceName,
                    autoGenerateServiceInstanceId: false);

            var tracerProviderBuilder = OpenTelemetry.Sdk
                .CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(endpointIdentity.LogicalName);

            if (configureTracingInstrumentation != null)
            {
                tracerProviderBuilder = configureTracingInstrumentation(tracerProviderBuilder);
            }

            var tracerProvider = tracerProviderBuilder
                .AddOtlpExporter()
                .Build() !;

            var tracer = tracerProvider.GetTracer(endpointIdentity.LogicalName, endpointIdentity.Version);

            var meterProviderBuilder = OpenTelemetry.Sdk
                .CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(endpointIdentity.LogicalName);

            if (configureMetricsInstrumentation != null)
            {
                meterProviderBuilder = configureMetricsInstrumentation(meterProviderBuilder);
            }

            var meterProvider = meterProviderBuilder
                .AddOtlpExporter()
                .Build() !;

            var meter = new Meter(endpointIdentity.LogicalName, endpointIdentity.Version);

            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(GenericEndpoint),
                    nameof(Telemetry)));

            return builder
                .ModifyContainerOptions(options => options
                    .WithPluginAssemblies(assembly)
                    .WithManualRegistrations(new TelemetryManualRegistration(tracerProvider, tracer, meterProvider, meter)));
        }
    }
}