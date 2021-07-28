namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// GroupByExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class GroupByExpression : ISubsequentIntermediateExpression,
                                     IEquatable<GroupByExpression>,
                                     ISafelyEquatable<GroupByExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        public GroupByExpression(Type itemType)
        {
            ItemType = itemType;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// GroupBy source
        /// </summary>
        public IIntermediateExpression Source { get; private set; } = null!;

        /// <summary>
        /// GroupBy key
        /// </summary>
        public ProjectionExpression Key { get; private set; } = null!;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left GroupByExpression</param>
        /// <param name="right">Right GroupByExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(GroupByExpression? left, GroupByExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left GroupByExpression</param>
        /// <param name="right">Right GroupByExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(GroupByExpression? left, GroupByExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Source);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(GroupByExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(GroupByExpression other)
        {
            return ItemType == other.ItemType
                   && Source.Equals(other.Source);
        }

        #endregion

        internal void Apply(ProjectionExpression expression)
        {
            if (Source == null)
            {
                Source = expression;
            }
            else if (Key == null)
            {
                Key = expression;
            }
        }

        internal void Apply(NamedSourceExpression expression)
        {
            Source = expression;
        }
    }
}