namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using GenericEndpoint;

    /// <summary>
    /// IntegrationMessageEventArgs
    /// </summary>
    public class IntegrationMessageEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Integration message</param>
        public IntegrationMessageEventArgs(IntegrationMessage message)
        {
            GeneralMessage = message;
        }

        /// <summary>
        /// General integration message
        /// </summary>
        public IntegrationMessage GeneralMessage { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return GeneralMessage.ToString();
        }
    }
}