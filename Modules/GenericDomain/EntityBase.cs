namespace SpaceEngineers.Core.GenericDomain
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// Base class for entities
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public abstract class EntityBase : IEntity,
                                       IEquatable<EntityBase>,
                                       ISafelyEquatable<EntityBase>
    {
        /// <summary>
        /// .cctor
        /// </summary>
        protected EntityBase()
        {
        }

        /// <summary>
        /// Entity identifier
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Entity identifier
        /// TODO: Versions / optimistic concurrency / historical entities
        /// </summary>
        public ulong Version { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left EntityBase</param>
        /// <param name="right">Right EntityBase</param>
        /// <returns>equals</returns>
        public static bool operator ==(EntityBase? left, EntityBase? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left EntityBase</param>
        /// <param name="right">Right EntityBase</param>
        /// <returns>not equals</returns>
        public static bool operator !=(EntityBase? left, EntityBase? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version, GetType());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(EntityBase? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool Equals(IEntity? other)
        {
            return Equatable.Equals((IEntity)this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(EntityBase other)
        {
            return ((IEntity)this).SafeEquals(other);
        }

        /// <inheritdoc />
        public bool SafeEquals(IEntity other)
        {
            return Id.Equals(other.Id)
                   && Version.Equals(other.Version)
                   && GetType() == other.GetType();
        }

        #endregion
    }
}