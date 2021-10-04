namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Basics;

    /// <summary>
    /// ViewNode
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ViewNode : IEquatable<ViewNode>,
                            ISafelyEquatable<ViewNode>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">View type</param>
        /// <param name="query">View query</param>
        public ViewNode(Type type, string query)
        {
            Type = type;
            Name = type.Name;
            Query = query;
        }

        /// <summary> .cctor </summary>
        /// <param name="name">View name</param>
        /// <param name="query">View query</param>
        public ViewNode(string name, string query)
        {
            Name = name;
            Query = query;
        }

        /// <summary>
        /// View type
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// View name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// View query
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
            return HashCode.Combine(Name, Query);
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
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Query.Equals(other.Query, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}