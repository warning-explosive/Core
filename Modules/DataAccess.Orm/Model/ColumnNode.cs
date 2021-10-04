namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// ColumnNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ColumnNode : IEquatable<ColumnNode>,
                              ISafelyEquatable<ColumnNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Column type</param>
        /// <param name="name">Column name</param>
        public ColumnNode(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Column type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Column name
        /// </summary>
        public string Name { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ColumnNode</param>
        /// <param name="right">Right ColumnNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(ColumnNode? left, ColumnNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ColumnNode</param>
        /// <param name="right">Right ColumnNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ColumnNode? left, ColumnNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ColumnNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ColumnNode other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}