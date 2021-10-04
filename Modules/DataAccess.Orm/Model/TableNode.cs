namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// TableNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class TableNode : IEquatable<TableNode>,
                             ISafelyEquatable<TableNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Table type</param>
        /// <param name="columns">Columns</param>
        public TableNode(Type type, IReadOnlyCollection<ColumnNode> columns)
        {
            Type = type;
            Name = type.Name;
            Columns = columns;
        }

        /// <summary> .cctor </summary>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns</param>
        public TableNode(string name, IReadOnlyCollection<ColumnNode> columns)
        {
            Name = name;
            Columns = columns;
        }

        /// <summary>
        /// Table type
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Columns
        /// </summary>
        public IReadOnlyCollection<ColumnNode> Columns { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left TableNode</param>
        /// <param name="right">Right TableNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(TableNode? left, TableNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left TableNode</param>
        /// <param name="right">Right TableNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(TableNode? left, TableNode? right)
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
        public bool Equals(TableNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(TableNode other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && ((Type == null && other.Type == null) || (Type == other.Type));
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}