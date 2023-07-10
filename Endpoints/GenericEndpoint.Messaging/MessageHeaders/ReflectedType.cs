namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// ReflectedType
    /// </summary>
    public record ReflectedType : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public ReflectedType()
        {
            Value = default!;
        }

        /// <summary> .cctor </summary>
        /// <param name="value">Reflected type</param>
        public ReflectedType(Type value)
        {
            Value = value;
        }

        /// <summary>
        /// Reflected type
        /// </summary>
        public Type Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value.FullName!;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ReflectedType)}:{StringValue}]";
        }
    }
}