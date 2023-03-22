namespace SpaceEngineers.Core.GenericEndpoint.Host.Registrations
{
    using CompositionRoot.Registration;
    using Endpoint;

    internal class TelemetryManualRegistration : IManualRegistration
    {
        private readonly ITelemetry _telemetry;

        public TelemetryManualRegistration(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public void Register(IManualRegistrationsContainer container)
        {
            container.RegisterInstance(_telemetry);
        }
    }
}