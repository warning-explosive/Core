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
    public class GroupByExpression : IIntermediateExpression,
                                     IEquatable<GroupByExpression>,
                                     ISafelyEquatable<GroupByExpression>,
                                     IApplicable<ProjectionExpression>,
                                     IApplicable<FilterExpression>,
                                     IApplicable<NamedSourceExpression>
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
        /// Group by keys
        /// </summary>
        public IIntermediateExpression Keys { get; private set; } = null!;

        /// <summary>
        /// Group by values
        /// </summary>
        public IIntermediateExpression Values { get; private set; } = null!;

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
            return HashCode.Combine(ItemType, Keys, Values);
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
                   && Keys.Equals(other.Keys)
                   && Values.Equals(other.Values);
        }

        #endregion

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression projection)
        {
            ApplyInternal(projection);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression filter)
        {
            ApplyInternal(filter);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression source)
        {
            ApplyInternal(source);
        }

        private void ApplyInternal(IIntermediateExpression expression)
        {
            if (Keys == null)
            {
                Keys = expression;
            }
            else if (Values == null)
            {
                Values = expression;
            }
        }
    }
}