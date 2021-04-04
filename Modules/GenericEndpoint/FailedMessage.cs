namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;

    /// <summary>
    /// Failed integration message
    /// </summary>
    public class FailedMessage
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Integration message that caused the error</param>
        /// <param name="exception">Processing error</param>
        public FailedMessage(IntegrationMessage message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Integration message that caused the error
        /// </summary>
        public IntegrationMessage Message { get; }

        /// <summary>
        /// Processing error
        /// </summary>
        public Exception Exception { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(Message.ToString(), Environment.NewLine, Exception.ToString());
        }
    }
}