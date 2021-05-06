namespace SpaceEngineers.Core.GenericDomain
{
    using System;
    using Abstractions;
    using Basics;

    /// <summary>
    /// Base class for entities
    /// </summary>
    public abstract class EntityBase : IEntity
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        /// <inheritdoc />
        public bool Equals(IEntity? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals<IEntity>(this, obj);
        }

        /// <inheritdoc />
        public bool SafeEquals(IEntity other)
        {
            return Id.Equals(other.Id)
                   && Version.Equals(other.Version);
        }
    }
}