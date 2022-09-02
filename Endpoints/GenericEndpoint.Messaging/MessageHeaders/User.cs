namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    /// <summary>
    /// User
    /// </summary>
    public class User : IIntegrationMessageHeader
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
        public string Value { get; }

        /// <inheritdoc />
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(User)}] - [{Value}]";
        }
    }
}