namespace SpaceEngineers.Core.Web.Api.Api
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Error
    /// </summary>
    [SuppressMessage("Analysis", "CA1716", Justification = "desired name")]
    public class Error
    {
        /// <summary> .cctor </summary>
        /// <param name="exception">Exception</param>
        public Error(Exception exception)
        {
            Message = exception.Message;
        }

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        public Error(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; }
    }
}