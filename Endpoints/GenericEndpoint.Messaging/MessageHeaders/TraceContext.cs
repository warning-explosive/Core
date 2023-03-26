namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    /// <summary>
    /// TraceContext
    /// </summary>
    public class TraceContext : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Trace context attributes</param>
        public TraceContext(Dictionary<string, object> value)
        {
            Value = value;
        }

        /// <summary>
        /// Trace context attributes
        /// </summary>
        public Dictionary<string, object> Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.Select(pair => pair.ToString()).ToString(", ");

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(TraceContext)}:{StringValue}]";
        }
    }
}