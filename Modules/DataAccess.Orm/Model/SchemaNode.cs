namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// SchemaNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class SchemaNode : IEquatable<SchemaNode>,
                              ISafelyEquatable<SchemaNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="name">Database name</param>
        /// <param name="tables">Database tables</param>
        /// <param name="views">Database views</param>
        public SchemaNode(string name,
            IReadOnlyCollection<TableNode> tables,
            IReadOnlyCollection<ViewNode> views)
        {
            Name = name;
            Tables = tables;
            Views = views;
        }

        /// <summary>
        /// Database name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Tables
        /// </summary>
        public IReadOnlyCollection<TableNode> Tables { get; }

        /// <summary>
        /// Views
        /// </summary>
        public IReadOnlyCollection<ViewNode> Views { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SchemaNode</param>
        /// <param name="right">Right SchemaNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(SchemaNode? left, SchemaNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SchemaNode</param>
        /// <param name="right">Right SchemaNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SchemaNode? left, SchemaNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SchemaNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SchemaNode other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}