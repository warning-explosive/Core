namespace SpaceEngineers.Core.GenericEndpoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contract.Abstractions;

    /// <summary>
    /// Integration message
    /// Class for technical purposes with headers support
    /// </summary>
    public class IntegrationMessage
    {
        /// <summary> .cctor </summary>
        /// <param name="payload">User-defined payload message</param>
        /// <param name="reflectedType">Message reflected type</param>
        public IntegrationMessage(
            IIntegrationMessage payload,
            Type reflectedType)
        {
            Payload = payload;
            ReflectedType = reflectedType;
            Headers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [IntegratedMessageHeader.ConversationId] = Guid.NewGuid()
            };
        }

        /// <summary>
        /// User-defined payload message
        /// </summary>
        public IIntegrationMessage Payload { get; }

        /// <summary>
        /// Message reflected type
        /// </summary>
        public Type ReflectedType { get; }

        /// <summary>
        /// Integration message headers
        /// </summary>
        public IDictionary<string, object> Headers { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var dict = new Dictionary<string, object>
            {
                ["Type"] = ReflectedType.Name,
                ["Payload"] = Payload
            };

            return string.Join(" ", dict.Concat(Headers));
        }
    }
}