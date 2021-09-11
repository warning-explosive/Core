namespace SpaceEngineers.Core.TracingEndpoint.Contract.Messages
{
    using System;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// CaptureTrace
    /// </summary>
    [OwnedBy(TracingEndpointIdentity.LogicalName)]
    public class CaptureTrace : IIntegrationCommand
    {
        /// <summary> .cctor </summary>
        /// <param name="integrationMessage">Integration message</param>
        /// <param name="exception">Exception</param>
        public CaptureTrace(
            IntegrationMessage integrationMessage,
            Exception? exception)
        {
            IntegrationMessage = integrationMessage;
            Exception = exception;
        }

        /// <summary>
        /// Integration message
        /// </summary>
        public IntegrationMessage IntegrationMessage { get; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception? Exception { get; }
    }
}