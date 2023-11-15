namespace SpaceEngineers.Core.GenericDomain.Api.Exceptions
{
    using System;

    /// <summary>
    /// DomainInvariantViolationException
    /// </summary>
    public sealed class DomainInvariantViolationException : Exception
    {
        /// <summary> .ctor </summary>
        /// <param name="message">Exception message</param>
        public DomainInvariantViolationException(string message)
            : base(message)
        {
        }
    }
}