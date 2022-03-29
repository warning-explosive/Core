namespace SpaceEngineers.Core.TracingEndpoint.Contract.Messages
{
    using System;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    /// <summary>
    /// CaptureTrace
    /// </summary>
    [OwnedBy(TracingEndpointIdentity.LogicalName)]
    public class CaptureTrace : IIntegrationCommand
    {
        /// <summary> .cctor </summary>
        /// <param name="serializedMessage">Serialized message</param>
        /// <param name="exception">Exception</param>
        public CaptureTrace(
            SerializedIntegrationMessage serializedMessage,
            Exception? exception)
        {
            SerializedMessage = serializedMessage;
            Exception = exception;
        }

        /// <summary>
        /// Serialized message
        /// </summary>
        public SerializedIntegrationMessage SerializedMessage { get; init; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception? Exception { get; init; }
    }
}