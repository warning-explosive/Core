namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Basics;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using MessageHeaders;

    /// <summary>
    /// Integration message
    /// Class for technical purposes with headers support
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class IntegrationMessage : IEquatable<IntegrationMessage>,
                                      ISafelyEquatable<IntegrationMessage>,
                                      ISafelyComparable<IntegrationMessage>,
                                      IComparable<IntegrationMessage>,
                                      IComparable,
                                      ICloneable<IntegrationMessage>
    {
        private readonly IStringFormatter _formatter;
        private readonly List<IIntegrationMessageHeader> _headers;

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

            _formatter = formatter;
            _headers = new List<IIntegrationMessageHeader>();
        }

        internal IntegrationMessage(
            Guid id,
            IIntegrationMessage payload,
            Type reflectedType,
            List<IIntegrationMessageHeader> headers,
            IStringFormatter formatter)
        {
            Id = id;
            Payload = payload;
            ReflectedType = reflectedType;
            _headers = headers;

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
        public IReadOnlyCollection<IIntegrationMessageHeader> Headers => _headers;

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
            return Id.CompareTo(other.Id);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            var headers = new List<IIntegrationMessageHeader>(Headers)
            {
                new ObjectHeader(nameof(ReflectedType), ReflectedType.Name),
                new ObjectHeader(nameof(Payload), Payload)
            };

            return FormatHeaders(headers);
        }

        /// <inheritdoc />
        public IntegrationMessage Clone()
        {
            return new IntegrationMessage(Id, Payload.DeepCopy(), ReflectedType, _headers.DeepCopy(), _formatter);
        }

        /// <inheritdoc />
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Reads header
        /// </summary>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        /// <returns>Header value</returns>
        public THeader? ReadHeader<THeader>()
            where THeader : IIntegrationMessageHeader
        {
            return Headers
                .OfType<THeader>()
                .InformativeSingleOrDefault(FormatHeaders);
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

            _headers.Add(header);
        }

        /// <summary>
        /// Overwrites existed header
        /// </summary>
        /// <param name="header">Header value</param>
        /// <typeparam name="THeader">THeader type-argument</typeparam>
        public void OverwriteHeader<THeader>(THeader header)
            where THeader : IIntegrationMessageHeader
        {
            var existedHeader = ReadHeader<THeader>();

            if (existedHeader != null)
            {
                _headers.Remove(existedHeader);
            }

            _headers.Add(header);
        }

        private string FormatHeaders<THeader>(IEnumerable<THeader> headers)
            where THeader : IIntegrationMessageHeader
        {
            return headers
                .Select(header => $"[{(header as ObjectHeader)?.Name ?? header.GetType().Name}, {_formatter.Format(header.Value)}]")
                .ToString(" ");
        }
    }
}