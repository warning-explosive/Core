namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using GenericEndpoint;

    /// <summary>
    /// FailedIntegrationMessageEventArgs
    /// </summary>
    public sealed class FailedIntegrationMessageEventArgs : IntegrationMessageEventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="failedMessage">Integration message that caused the error</param>
        public FailedIntegrationMessageEventArgs(FailedMessage failedMessage)
            : base(failedMessage.Message)
        {
            Exception = failedMessage.Exception;
        }

        /// <summary>
        /// Processing error
        /// </summary>
        public Exception Exception { get; }
    }
}