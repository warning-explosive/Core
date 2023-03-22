namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using OpenTelemetry.Trace;

    /// <summary>
    /// ITelemetry
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// Tracer
        /// </summary>
        Tracer Tracer { get; }
    }
}