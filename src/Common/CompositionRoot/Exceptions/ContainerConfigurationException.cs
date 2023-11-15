namespace SpaceEngineers.Core.CompositionRoot.Exceptions
{
    using System;

    /// <summary>
    /// ContainerConfigurationException
    /// </summary>
    public sealed class ContainerConfigurationException : Exception
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public ContainerConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="message">Message</param>
        public ContainerConfigurationException(string message)
            : base(message, null)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="inner">Inner exception</param>
        public ContainerConfigurationException(Exception inner)
            : base("Container has invalid configuration", inner)
        {
        }
    }
}