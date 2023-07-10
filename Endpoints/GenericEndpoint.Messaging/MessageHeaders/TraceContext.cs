namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Basics;

    /// <summary>
    /// TraceContext
    /// </summary>
    public record TraceContext : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public TraceContext()
        {
            Value = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="value">Trace context attributes</param>
        public TraceContext(Dictionary<string, object> value)
        {
            Value = value;
        }

        /// <summary>
        /// Trace context attributes
        /// </summary>
        public Dictionary<string, object> Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.Select(pair => pair.ToString()).ToString(", ");

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(TraceContext)}:{StringValue}]";
        }
    }
}