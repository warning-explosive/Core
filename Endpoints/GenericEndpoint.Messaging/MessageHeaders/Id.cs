namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;

    /// <summary>
    /// Message Id
    /// </summary>
    public class Id : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Initiator message id</param>
        public Id(Guid value)
        {
            Value = value;
        }

        /// <summary>
        /// Message id
        /// </summary>
        public Guid Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.ToString();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(Id)}:{StringValue}]";
        }
    }
}