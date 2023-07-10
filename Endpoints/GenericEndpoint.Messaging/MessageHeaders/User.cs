namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// User
    /// </summary>
    public record User : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">User</param>
        public User(string value)
        {
            Value = value;
        }

        /// <summary>
        /// User
        /// </summary>
        public string Value { get; init; }

        /// <inheritdoc />
        [JsonIgnore]
        public string StringValue => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(User)}:{StringValue}]";
        }
    }
}