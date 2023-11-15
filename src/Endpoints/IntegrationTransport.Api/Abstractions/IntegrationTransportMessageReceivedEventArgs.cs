namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IntegrationTransportMessageReceivedEventArgs
    /// </summary>
    public class IntegrationTransportMessageReceivedEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="message">IntegrationMessage</param>
        /// <param name="exception">Exception</param>
        public IntegrationTransportMessageReceivedEventArgs(
            IntegrationMessage message,
            Exception? exception)
        {
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Integration message
        /// </summary>
        public IntegrationMessage Message { get; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception? Exception { get; }
    }
}