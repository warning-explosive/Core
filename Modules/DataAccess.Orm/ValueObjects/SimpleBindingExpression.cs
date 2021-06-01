namespace SpaceEngineers.Core.DataAccess.Orm.ValueObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// SimpleBindingExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class SimpleBindingExpression : INamedIntermediateExpression,
                                           IEquatable<SimpleBindingExpression>,
                                           ISafelyEquatable<SimpleBindingExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="name">Name</param>
        /// <param name="expression">IIntermediateExpression</param>
        public SimpleBindingExpression(
            Type itemType,
            string name,
            IIntermediateExpression expression)
        {
            ItemType = itemType;
            Name = name;
            Expression = expression;
        }

        internal SimpleBindingExpression(Type itemType, string name)
        {
            ItemType = itemType;
            Name = name;
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IIntermediateExpression Expression { get; private set; } = null!;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left SimpleBindingExpression</param>
        /// <param name="right">Right SimpleBindingExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(SimpleBindingExpression? left, SimpleBindingExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left SimpleBindingExpression</param>
        /// <param name="right">Right SimpleBindingExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(SimpleBindingExpression? left, SimpleBindingExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Name, Expression);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(SimpleBindingExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(SimpleBindingExpression other)
        {
            return ItemType == other.ItemType
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Expression.Equals(other.Expression);
        }

        #endregion

        internal void Apply(SimpleBindingExpression binding)
        {
            Expression = binding;
        }

        internal void Apply(ParameterExpression parameter)
        {
            Expression = parameter;
        }
    }
}