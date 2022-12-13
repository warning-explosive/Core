namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// DatabaseNode
    /// </summary>
    public class DatabaseNode : IEquatable<DatabaseNode>,
                                ISafelyEquatable<DatabaseNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="host">Host</param>
        /// <param name="name">Database name</param>
        /// <param name="schemas">Database schemas</param>
        public DatabaseNode(
            string host,
            string name,
            IReadOnlyCollection<SchemaNode> schemas)
        {
            Host = host;
            Name = name;
            Schemas = schemas;
        }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Database name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Database schemas
        /// </summary>
        public IReadOnlyCollection<SchemaNode> Schemas { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left DatabaseNode</param>
        /// <param name="right">Right DatabaseNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(DatabaseNode? left, DatabaseNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left DatabaseNode</param>
        /// <param name="right">Right DatabaseNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(DatabaseNode? left, DatabaseNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        [SuppressMessage("Analysis", "CA1308", Justification = "sql script readability")]
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Host.ToLowerInvariant(),
                Name.ToLowerInvariant());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(DatabaseNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(DatabaseNode other)
        {
            return Host.Equals(other.Host, StringComparison.OrdinalIgnoreCase)
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Host}/{Name}";
        }
    }
}