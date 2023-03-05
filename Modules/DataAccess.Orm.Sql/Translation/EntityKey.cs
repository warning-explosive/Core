namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Api.Model;

    internal class EntityKey : IEquatable<EntityKey>,
                               ISafelyEquatable<EntityKey>
    {
        public EntityKey(IUniqueIdentified entity)
            : this(entity.GetType(), entity.PrimaryKey)
        {
        }

        public EntityKey(Type type, object primaryKey)
        {
            Type = type;
            PrimaryKey = primaryKey;
        }

        public Type Type { get; }

        public object PrimaryKey { get; }

        #region IEquatable

        public static bool operator ==(EntityKey? left, EntityKey? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(EntityKey? left, EntityKey? right)
        {
            return !Equatable.Equals(left, right);
        }

        public bool SafeEquals(EntityKey other)
        {
            return Type == other.Type
                   && PrimaryKey.Equals(other.PrimaryKey);
        }

        public bool Equals(EntityKey? other)
        {
            return Equatable.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, PrimaryKey);
        }

        #endregion
    }
}