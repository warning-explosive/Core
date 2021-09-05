namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using Abstractions;

    /// <summary>
    /// Retry counter
    /// </summary>
    public class RetryCounter : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Retry counter</param>
        public RetryCounter(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Retry counter
        /// </summary>
        public int Value { get; }

        /// <inheritdoc />
        object IIntegrationMessageHeader.Value => Value;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(RetryCounter)}] - [{Value}]";
        }
    }
}