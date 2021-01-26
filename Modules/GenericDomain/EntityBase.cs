namespace SpaceEngineers.Core.GenericDomain
{
    using System;
    using Abstractions;

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
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Entity identifier
        /// </summary>
        public Guid Id { get; private set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(IEntity? other)
        {
            return Equals((object?)other);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((IUniqueIdentified)obj);
        }

        private bool Equals(IUniqueIdentified other)
        {
            return Id.Equals(other.Id);
        }
    }
}