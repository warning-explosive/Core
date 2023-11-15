namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using Basics;

    internal class Relation : IEquatable<Relation>,
                              ISafelyEquatable<Relation>
    {
        private readonly IModelProvider _modelProvider;

        public Relation(
            Type source,
            Type target,
            ColumnProperty property,
            IModelProvider modelProvider)
        {
            Source = source;
            Target = target;
            Property = property;

            _modelProvider = modelProvider;
        }

        public Type Source { get; }

        public Type Target { get; }

        public ColumnProperty Property { get; }

        #region IEquatable

        public static bool operator ==(Relation? left, Relation? right)
        {
            return Equatable.Equals(left, right);
        }

        public static bool operator !=(Relation? left, Relation? right)
        {
            return !Equatable.Equals(left, right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Target, Property);
        }

        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        public bool Equals(Relation? other)
        {
            return Equatable.Equals(this, other);
        }

        public bool SafeEquals(Relation other)
        {
            return Source == other.Source
                   && Target == other.Target
                   && Property == other.Property;
        }

        #endregion

        public override string ToString()
        {
            return $"{_modelProvider.TableName(Source)} -> {_modelProvider.TableName(Target)} ({Property.Name})";
        }
    }
}