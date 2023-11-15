namespace SpaceEngineers.Core.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base response
    /// </summary>
    public abstract class BaseResponse : IResponse
    {
        private List<Error> _errors;

        /// <summary>
        /// .cctor
        /// </summary>
        protected BaseResponse()
        {
            _errors = new List<Error>();
        }

        /// <inheritdoc />
        public bool Success => !Errors.Any();

        /// <inheritdoc />
        public IReadOnlyCollection<Error> Errors => _errors;

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="exception">Exception</param>
        protected void AddError(Exception exception)
        {
            _errors.Add(new Error(exception));
        }

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="message">Message</param>
        protected void AddError(string message)
        {
            _errors.Add(new Error(message));
        }
    }
}