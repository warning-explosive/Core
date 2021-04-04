namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using System;
    using System.Collections.Generic;
    using GenericEndpoint;

    /// <summary>
    /// ITransportStatistics abstraction
    /// </summary>
    public interface IHostStatistics
    {
        /// <summary>
        /// Failed messages
        /// </summary>
        IReadOnlyCollection<FailedMessage> FailedMessages { get; }

        /// <summary>
        /// Registers failed message
        /// </summary>
        /// <param name="message">Integration message that caused the error</param>
        /// <param name="exception">Processing error</param>
        /// <returns>IHostStatistics</returns>
        public IHostStatistics RegisterFailure(IntegrationMessage message, Exception exception);
    }
}