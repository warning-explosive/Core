namespace SpaceEngineers.Core.GenericEndpoint.Messaging.MessageHeaders
{
    using System;

    /// <summary>
    /// ReflectedType
    /// </summary>
    public class ReflectedType : IIntegrationMessageHeader
    {
        /// <summary> .cctor </summary>
        /// <param name="value">Reflected type</param>
        public ReflectedType(Type value)
        {
            Value = value;
        }

        /// <summary>
        /// Reflected type
        /// </summary>
        public Type Value { get; }

        /// <inheritdoc />
        public string StringValue => Value.FullName!;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{nameof(ReflectedType)}:{StringValue}]";
        }
    }
}