namespace SpaceEngineers.Core.GenericEndpoint.Telemetry.Host
{
    using System.Diagnostics.Metrics;
    using CompositionRoot.Registration;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Trace;

    internal class TelemetryManualRegistration : IManualRegistration
    {
        private readonly TracerProvider _tracerProvider;
        private readonly Tracer _tracer;
        private readonly MeterProvider _meterProvider;
        private readonly Meter _meter;

        public TelemetryManualRegistration(
            TracerProvider tracerProvider,
            Tracer tracer,
            MeterProvider meterProvider,
            Meter meter)
        {
            _tracerProvider = tracerProvider;
            _tracer = tracer;
            _meterProvider = meterProvider;
            _meter = meter;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_tracerProvider);
            container.RegisterInstance(_tracer);
            container.RegisterInstance(_meterProvider);
            container.RegisterInstance(_meter);
        }
    }
}