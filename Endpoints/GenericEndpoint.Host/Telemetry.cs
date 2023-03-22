namespace SpaceEngineers.Core.GenericEndpoint.Host
{
    using System;
    using System.Diagnostics.Metrics;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Endpoint;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    internal class Telemetry : ITelemetry,
                               IDisposable,
                               IResolvable<ITelemetry>
    {
        private readonly TracerProvider _tracerProvider;
        private readonly MeterProvider _meterProvider;

        public Telemetry(
            TracerProvider tracerProvider,
            Tracer tracer,
            MeterProvider meterProvider,
            Meter meter)
        {
            _tracerProvider = tracerProvider;
            Tracer = tracer;

            _meterProvider = meterProvider;
            Meter = meter;
        }

        public Tracer Tracer { get; }

        private Meter Meter { get; }

        public void Dispose()
        {
            _tracerProvider.Dispose();

            Meter.Dispose();

            _meterProvider.Dispose();
        }
    }
}