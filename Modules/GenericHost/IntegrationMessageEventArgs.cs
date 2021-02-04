namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// IntegrationMessageEventArgs
    /// </summary>
    public class IntegrationMessageEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Integration message</param>
        /// <param name="reflectedType">Reflected type</param>
        public IntegrationMessageEventArgs(
            IIntegrationMessage message,
            Type reflectedType)
        {
            Message = message;
            ReflectedType = reflectedType;
        }

        /// <summary>
        /// Integration message
        /// </summary>
        public IIntegrationMessage Message { get; }

        /// <summary>
        /// Reflected type
        /// </summary>
        public Type ReflectedType { get; }
    }
}