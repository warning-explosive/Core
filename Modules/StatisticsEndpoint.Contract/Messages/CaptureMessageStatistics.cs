namespace SpaceEngineers.Core.StatisticsEndpoint.Contract.Messages
{
    using System;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// CaptureMessageStatistics command
    /// </summary>
    [OwnedBy(StatisticsEndpointIdentity.LogicalName)]
    public class CaptureMessageStatistics : IIntegrationCommand
    {
        /// <summary> .cctor </summary>
        /// <param name="generalMessage">Integration message</param>
        public CaptureMessageStatistics(IntegrationMessage generalMessage)
        {
            GeneralMessage = generalMessage;
        }

        /// <summary>
        /// Integration message
        /// </summary>
        public IntegrationMessage GeneralMessage { get; private set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception? Exception { get; set; }
    }
}