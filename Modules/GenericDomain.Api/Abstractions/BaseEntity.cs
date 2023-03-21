namespace SpaceEngineers.Core.GenericDomain.Api.Abstractions
{
    using System;
    using Basics;

    /// <summary>
    /// BaseEntity
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary> .cctor </summary>
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        /// <inheritdoc />
        public Guid Id { get; protected set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left IEntity</param>
        /// <param name="right">Right IEntity</param>
        /// <returns>equals</returns>
        public static bool operator ==(BaseEntity? left, BaseEntity? right)
        {
            return Equatable.Equals<IEntity>(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left BaseEntity</param>
        /// <param name="right">Right BaseEntity</param>
        /// <returns>not equals</returns>
        public static bool operator !=(BaseEntity? left, BaseEntity? right)
        {
            return !Equatable.Equals<IEntity>(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GetType());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals<IEntity>(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(IEntity? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(IEntity? other)
        {
            return Id.Equals(other.Id);
        }

        #endregion
    }
}