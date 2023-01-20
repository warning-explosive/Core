namespace SpaceEngineers.Core.AuthEndpoint.Domain
{
    using System;
    using System.Diagnostics;
    using Basics;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.Api.Exceptions;

    /// <summary>
    /// Feature
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class Feature : IValueObject,
                           IEquatable<Feature>,
                           ISafelyEquatable<Feature>
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Feature name</param>
        public Feature(string name)
        {
            if (name.IsNullOrWhiteSpace())
            {
                throw new DomainInvariantViolationException("Feature should have non-empty name");
            }

            Name = name;
        }

        /// <summary>
        /// Feature name
        /// </summary>
        public string Name { get; init; }

        #region IEquatable

        /// <inheritdoc />
        public bool SafeEquals(Feature other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool Equals(Feature? other)
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
            return Name.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}