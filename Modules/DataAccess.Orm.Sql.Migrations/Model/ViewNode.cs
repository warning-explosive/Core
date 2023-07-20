namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using Basics;

    /// <summary>
    /// ViewNode
    /// </summary>
    public class ViewNode : IEquatable<ViewNode>,
                            ISafelyEquatable<ViewNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="schema">Schema</param>
        /// <param name="view">View</param>
        /// <param name="query">Query</param>
        public ViewNode(
            string schema,
            string view,
            string query)
        {
            Schema = schema;
            View = view;
            Query = query;
        }

        /// <summary>
        /// Schema
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// View
        /// </summary>
        public string View { get; }

        /// <summary>
        /// Query
        /// </summary>
        public string Query { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ViewNode</param>
        /// <param name="right">Right ViewNode</param>
        /// <returns>equals</returns>
        public static bool operator ==(ViewNode? left, ViewNode? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ViewNode</param>
        /// <param name="right">Right ViewNode</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ViewNode? left, ViewNode? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Schema.GetHashCode(StringComparison.OrdinalIgnoreCase),
                View.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Query.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ViewNode? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ViewNode other)
        {
            return Schema.Equals(other.Schema, StringComparison.OrdinalIgnoreCase)
                   && View.Equals(other.View, StringComparison.OrdinalIgnoreCase)
                   && Query.Equals(other.Query, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Schema}.{View} ({Query})";
        }
    }
}