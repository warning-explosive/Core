namespace SpaceEngineers.Core.Web.Api.Api
{
    using System;

    /// <summary>
    /// Empty response
    /// </summary>
    public class EmptyResponse : BaseResponse
    {
        /// <summary>
        /// With error
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>EmptyResponse</returns>
        public EmptyResponse WithError(Exception exception)
        {
            AddError(exception);
            return this;
        }

        /// <summary>
        /// With error
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>EmptyResponse</returns>
        public EmptyResponse WithError(string message)
        {
            AddError(message);
            return this;
        }
    }
}