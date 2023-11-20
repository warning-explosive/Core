namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contract;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    internal class TelemetryProvider : ITelemetryProvider
    {
        private readonly IReadOnlyDictionary<EndpointIdentity, TracerProvider> _tracerProviders;
        private readonly IReadOnlyDictionary<EndpointIdentity, MeterProvider> _meterProviders;

        public TelemetryProvider(
            IReadOnlyDictionary<EndpointIdentity, TracerProvider> tracerProviders,
            IReadOnlyDictionary<EndpointIdentity, MeterProvider> meterProviders)
        {
            _tracerProviders = tracerProviders;
            _meterProviders = meterProviders;
        }

        public bool TryGetTracerProvider(
            EndpointIdentity endpointIdentity,
            [NotNullWhen(true)] out TracerProvider? tracerProvider)
        {
            return _tracerProviders.TryGetValue(endpointIdentity, out tracerProvider);
        }

        public bool TryGetMeterProvider(
            EndpointIdentity endpointIdentity,
            [NotNullWhen(true)] out MeterProvider? meterProvider)
        {
            return _meterProviders.TryGetValue(endpointIdentity, out meterProvider);
        }
    }
}