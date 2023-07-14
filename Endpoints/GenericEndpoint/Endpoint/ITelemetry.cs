namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using OpenTelemetry.Trace;

    /// <summary>
    /// ITelemetry
    /// </summary>
    // TODO: #226 - move tracing outside of endpoint scope and pass in through builder as framework dependency on a par with logger
    public interface ITelemetry
    {
        /// <summary>
        /// Tracer
        /// </summary>
        Tracer Tracer { get; }
    }
}