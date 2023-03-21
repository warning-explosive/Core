namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;
    using Contract.Abstractions;
    using MessageHeaders;

    /// <summary>
    /// IIntegrationMessageFactory abstraction
    /// </summary>
    public interface IIntegrationMessageFactory
    {
        /// <summary>
        /// Creates IntegrationMessage instance from user defined payload
        /// </summary>
        /// <param name="payload">User defined payload message</param>
        /// <param name="reflectedType">Reflected type</param>
        /// <param name="headers">Integration message headers</param>
        /// <param name="initiatorMessage">Initiator integration message</param>
        /// <returns>IntegrationMessage instance</returns>
        IntegrationMessage CreateGeneralMessage(
            IIntegrationMessage payload,
            Type reflectedType,
            IReadOnlyCollection<IIntegrationMessageHeader> headers,
            IntegrationMessage? initiatorMessage);
    }
}