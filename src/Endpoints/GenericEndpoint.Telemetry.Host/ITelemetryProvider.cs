namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System.Diagnostics.CodeAnalysis;
    using Contract;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    internal interface ITelemetryProvider
    {
        public bool TryGetTracerProvider(
            EndpointIdentity endpointIdentity,
            [NotNullWhen(true)] out TracerProvider? tracerProvider);

        public bool TryGetMeterProvider(
            EndpointIdentity endpointIdentity,
            [NotNullWhen(true)] out MeterProvider? tracerProvider);
    }
}