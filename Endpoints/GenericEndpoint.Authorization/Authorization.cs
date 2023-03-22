namespace SpaceEngineers.Core.GenericEndpoint.Authorization
{
    using System.Diagnostics.CodeAnalysis;
    using Messaging.MessageHeaders;

    /// <summary>
    /// Authorization
    /// </summary>
    [SuppressMessage("Analysis", "CA1724", Justification = "desired name")]
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
        public string StringValue => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(Authorization)}:{StringValue}]";
        }
    }
}