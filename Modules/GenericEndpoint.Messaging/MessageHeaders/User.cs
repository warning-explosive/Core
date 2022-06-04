namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;

    /// <summary>
    /// User
    /// </summary>
    public class User : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="user">User</param>
        public User(string user)
        {
            Value = user;
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