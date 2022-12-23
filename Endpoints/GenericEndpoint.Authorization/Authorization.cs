namespace SpaceEngineers.Core.GenericEndpoint.Authorization
{
    using Messaging.MessageHeaders;

    /// <summary>
    /// Authorization
    /// </summary>
    public class Authorization : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Authorization token</param>
        public Authorization(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Authorization token
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(Authorization)}] - [{Value}]";
        }
    }
}