namespace SpaceEngineers.Core.CompositionRoot.Api.Exceptions
{
    using System;

    /// <summary>
    /// ComponentResolutionException
    /// </summary>
    public sealed class ComponentResolutionException : Exception
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public ComponentResolutionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        public ComponentResolutionException(string message)
            : base(message, null)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="service">service</param>
        /// <param name="inner">Inner exception</param>
        public ComponentResolutionException(Type service, Exception inner)
            : base($"Component for service {service} wasn't resolved", inner)
        {
        }
    }
}