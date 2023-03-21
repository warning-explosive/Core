namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System;
    using Enumerations;

    /// <summary>
    /// IntegrationTransportStatusChangedEventArgs
    /// </summary>
    public class IntegrationTransportStatusChangedEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="previousStatus">Previous status</param>
        /// <param name="currentStatus">Current status</param>
        public IntegrationTransportStatusChangedEventArgs(
            EnIntegrationTransportStatus previousStatus,
            EnIntegrationTransportStatus currentStatus)
        {
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
        }

        /// <summary>
        /// Previous status
        /// </summary>
        public EnIntegrationTransportStatus PreviousStatus { get; }

        /// <summary>
        /// Current status
        /// </summary>
        public EnIntegrationTransportStatus CurrentStatus { get; }
    }
}