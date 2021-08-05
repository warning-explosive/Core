namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;

    /// <summary>
    /// Integration message
    /// Class for technical purposes with headers support
    /// </summary>
    public class IntegrationMessage : IEquatable<IntegrationMessage>,
                                      ISafelyEquatable<IntegrationMessage>,
                                      ISafelyComparable<IntegrationMessage>,
                                      IComparable<IntegrationMessage>,
                                      IComparable,
                                      ICloneable<IntegrationMessage>
    {
        private readonly IStringFormatter _formatter;

        /// <summary> .cctor </summary>
        /// <param name="payload">User-defined payload message</param>
        /// <param name="reflectedType">Message reflected type</param>
        /// <param name="formatter">IStringFormatter</param>
        public IntegrationMessage(
            IIntegrationMessage payload,
            Type reflectedType,
            IStringFormatter formatter)
        {
            Id = Guid.NewGuid();
            Payload = payload;
            ReflectedType = reflectedType;
            Headers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [IntegrationMessageHeader.ConversationId] = Guid.NewGuid()
            };

            _formatter = formatter;
        }

        /// <summary>
        /// Copy .cctor
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="payload">Payload</param>
        /// <param name="reflectedType">Reflected type</param>
        /// <param name="headers">Headers</param>
        /// <param name="formatter">To string formatter</param>
        private IntegrationMessage(
            Guid id,
            IIntegrationMessage payload,
            Type reflectedType,
            IDictionary<string, object> headers,
            IStringFormatter formatter)
        {
            Id = id;
            Payload = payload;
            ReflectedType = reflectedType;
            Headers = headers;

            _formatter = formatter;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// User-defined payload message
        /// </summary>
        public IIntegrationMessage Payload { get; }

        /// <summary>
        /// Message reflected type
        /// </summary>
        public Type ReflectedType { get; }

        /// <summary>
        /// Integration message headers
        /// </summary>
        public IDictionary<string, object> Headers { get; }

        /// <summary>
        /// Equality ==
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator ==(IntegrationMessage? left, IntegrationMessage? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// Equality !=
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator !=(IntegrationMessage? left, IntegrationMessage? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <summary>
        /// Less operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator <(IntegrationMessage? left, IntegrationMessage? right)
        {
            return Comparable.Less(left, right);
        }

        /// <summary>
        /// Greater operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator >(IntegrationMessage? left, IntegrationMessage? right)
        {
            return Comparable.Greater(left, right);
        }

        /// <summary>
        /// Less or equals operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator <=(IntegrationMessage? left, IntegrationMessage? right)
        {
            return Comparable.LessOrEquals(left, right);
        }

        /// <summary>
        /// Greater or equals operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Operation result</returns>
        public static bool operator >=(IntegrationMessage? left, IntegrationMessage? right)
        {
            return Comparable.GreaterOrEquals(left, right);
        }

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            return Comparable.CompareTo(this, obj);
        }

        /// <inheritdoc />
        public int CompareTo(IntegrationMessage? other)
        {
            return Comparable.CompareTo(this, other);
        }

        /// <inheritdoc />
        public int SafeCompareTo(IntegrationMessage other)
        {
            return Id.CompareTo(other.Id);
        }

        /// <inheritdoc />
        public bool SafeEquals(IntegrationMessage other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public bool Equals(IntegrationMessage? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var headers = new Dictionary<string, object>(Headers)
            {
                [nameof(ReflectedType)] = ReflectedType.Name,
                [nameof(Payload)] = Payload
            };

            return headers
                .Select(pair => new
                {
                    pair.Key,
                    Value = _formatter.Format(pair.Value)
                })
                .Select(pair => $"[{pair.Key}, {pair.Value}]")
                .ToString(" ");
        }

        /// <inheritdoc />
        public IntegrationMessage Clone()
        {
            return new IntegrationMessage(Id, Payload.DeepCopy(), ReflectedType, Headers.DeepCopy(), _formatter);
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}