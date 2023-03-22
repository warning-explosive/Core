namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using MessageHeaders;

    /// <summary>
    /// Integration message
    /// Class for technical purposes with headers support
    /// </summary>
    public class IntegrationMessage : IEquatable<IntegrationMessage>,
                                      ISafelyEquatable<IntegrationMessage>,
                                      ISafelyComparable<IntegrationMessage>,
                                      IComparable<IntegrationMessage>,
                                      IComparable
    {
        private readonly Dictionary<Type, IIntegrationMessageHeader> _headers;

        /// <summary> .cctor </summary>
        /// <param name="payload">User-defined payload message</param>
        /// <param name="reflectedType">Message reflected type</param>
        public IntegrationMessage(
            IIntegrationMessage payload,
            Type reflectedType)
            : this(payload, reflectedType, new Dictionary<Type, IIntegrationMessageHeader>())
        {
        }

        internal IntegrationMessage(
            IIntegrationMessage payload,
            Type reflectedType,
            Dictionary<Type, IIntegrationMessageHeader> headers)
        {
            Payload = payload;
            ReflectedType = reflectedType;

            if (!headers.ContainsKey(typeof(Id)))
            {
                headers[typeof(Id)] = new Id(Guid.NewGuid());
            }

            if (!headers.ContainsKey(typeof(ReflectedType)))
            {
                headers[typeof(ReflectedType)] = new ReflectedType(reflectedType);
            }

            _headers = headers;
        }

        /// <summary>
        /// User-defined payload message
        /// </summary>
        public IIntegrationMessage Payload { get; init; }

        /// <summary>
        /// Message reflected type
        /// </summary>
        public Type ReflectedType { get; init; }

        /// <summary>
        /// Integration message headers
        /// </summary>
        public IReadOnlyDictionary<Type, IIntegrationMessageHeader> Headers
        {
            get => _headers;
            init => _headers = (Dictionary<Type, IIntegrationMessageHeader>)value;
        }

        #region IEquitable

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
        public bool SafeEquals(IntegrationMessage other)
        {
            return ReadRequiredHeader<Id>().Value == other.ReadRequiredHeader<Id>().Value;
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
            return ReadRequiredHeader<Id>().Value.GetHashCode();
        }

        #endregion

        #region IComparable

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
            return ReadRequiredHeader<Id>().Value.CompareTo(other.ReadRequiredHeader<Id>().Value);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Headers
                .Select(header => header.Value.ToString())
                .ToString(" ");
        }

        /// <summary>
        /// Clones original message with specified contravariant type
        /// </summary>
        /// <param name="reflectedType">Reflected type</param>
        /// <returns>Copy</returns>
        public IntegrationMessage ContravariantClone(Type reflectedType)
        {
            if (!reflectedType.IsAssignableFrom(ReflectedType)
                || !reflectedType.IsConstructedOrNonGenericType())
            {
                throw new InvalidOperationException($"{reflectedType} isn't suitable as contravariant analogue for {ReflectedType}");
            }

            return new IntegrationMessage(Payload, reflectedType, _headers.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        /// <summary>
        /// Gets target endpoint logical name
        /// </summary>
        /// <returns>Target endpoint logical name</returns>
        public string GetTargetEndpoint()
        {
            var targetEndpoint = this.IsEvent()
                ? "*"
                : ReadHeader<ReplyTo>()?.Value.LogicalName ?? ReflectedType.GetRequiredAttribute<OwnedByAttribute>().EndpointName;

            return targetEndpoint.Equals(nameof(EndpointIdentity), StringComparison.OrdinalIgnoreCase)
                ? ReadRequiredHeader<SentFrom>().Value.LogicalName
                : targetEndpoint;
        }

        /// <summary>
        /// Reads header
        /// </summary>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        /// <returns>Header value</returns>
        public THeader? ReadHeader<THeader>()
            where THeader : IIntegrationMessageHeader
        {
            return Headers.TryGetValue(typeof(THeader), out var integrationMessageHeader)
                && integrationMessageHeader is THeader header
                ? header
                : default;
        }

        /// <summary>
        /// Reads required header
        /// </summary>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        /// <returns>Header value</returns>
        public THeader ReadRequiredHeader<THeader>()
            where THeader : IIntegrationMessageHeader
        {
            return ReadHeader<THeader>()
                   ?? throw new InvalidOperationException($"Message should have {typeof(THeader).Name} message header");
        }

        /// <summary>
        /// Writes header
        /// </summary>
        /// <param name="header">Header value</param>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        public void WriteHeader<THeader>(THeader header)
            where THeader : IIntegrationMessageHeader
        {
            var existedHeader = ReadHeader<THeader>();

            if (existedHeader != null)
            {
                throw new InvalidOperationException($"Header {typeof(THeader).Name} already exists in the message");
            }

            _headers.Add(header.GetType(), header);
        }

        /// <summary>
        /// Overwrites existed header
        /// </summary>
        /// <param name="header">Header value</param>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        public void OverwriteHeader<THeader>(THeader header)
            where THeader : IIntegrationMessageHeader
        {
            _headers[header.GetType()] = header;
        }
    }
}