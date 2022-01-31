namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Basics;

    /// <summary>
    /// Relation
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class Relation : IEquatable<Relation>,
                            ISafelyEquatable<Relation>
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source type</param>
        /// <param name="target">Target type</param>
        /// <param name="property">Property</param>
        public Relation(
            Type source,
            Type target,
            PropertyInfo property)
        {
            Source = source;
            Target = target;
            Property = property;
        }

        /// <summary>
        /// Source type
        /// </summary>
        public Type Source { get; }

        /// <summary>
        /// Target type
        /// </summary>
        public Type Target { get; }

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfo Property { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left Relation</param>
        /// <param name="right">Right Relation</param>
        /// <returns>equals</returns>
        public static bool operator ==(Relation? left, Relation? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left Relation</param>
        /// <param name="right">Right Relation</param>
        /// <returns>not equals</returns>
        public static bool operator !=(Relation? left, Relation? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Target, Property);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(Relation? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(Relation other)
        {
            return Source == other.Source
                   && Target == other.Target
                   && Property == other.Property;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source.TableName()} -> {Target.FullName} ({Property.Name})";
        }
    }
}