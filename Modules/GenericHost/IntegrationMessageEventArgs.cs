namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using Basics;
    using GenericEndpoint;

    /// <summary>
    /// IntegrationMessageEventArgs
    /// </summary>
    public class IntegrationMessageEventArgs : EventArgs,
                                               IEquatable<IntegrationMessageEventArgs>,
                                               ISafelyEquatable<IntegrationMessageEventArgs>,
                                               ISafelyComparable<IntegrationMessageEventArgs>,
                                               IComparable<IntegrationMessageEventArgs>,
                                               IComparable
    {
        /// <summary> .cctor </summary>
        /// <param name="message">Integration message</param>
        public IntegrationMessageEventArgs(IntegrationMessage message)
        {
            GeneralMessage = message;
        }

        /// <summary>
        /// General integration message
        /// </summary>
        public IntegrationMessage GeneralMessage { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return GeneralMessage.ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GeneralMessage.GetHashCode();
        }

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            return Comparable.CompareTo(this, obj);
        }

        /// <inheritdoc />
        public int CompareTo(IntegrationMessageEventArgs? other)
        {
            return Comparable.CompareTo(this, other);
        }

        /// <inheritdoc />
        public int SafeCompareTo(IntegrationMessageEventArgs other)
        {
            return GeneralMessage.CompareTo(other.GeneralMessage);
        }

        /// <inheritdoc />
        public bool SafeEquals(IntegrationMessageEventArgs other)
        {
            return GeneralMessage.Equals(other.GeneralMessage);
        }

        /// <inheritdoc />
        public bool Equals(IntegrationMessageEventArgs? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }
    }
}