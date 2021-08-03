namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Basics;

    /// <summary>
    /// MethodCallExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class MethodCallExpression : IIntermediateExpression,
                                        IEquatable<MethodCallExpression>,
                                        ISafelyEquatable<MethodCallExpression>,
                                        IApplicable<SimpleBindingExpression>
    {
        private readonly List<IIntermediateExpression> _arguments;

        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="name">Name</param>
        /// <param name="arguments">Arguments</param>
        public MethodCallExpression(
            Type itemType,
            string name,
            IEnumerable<IIntermediateExpression> arguments)
        {
            ItemType = itemType;
            Name = name;
            _arguments = arguments.ToList();
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IReadOnlyCollection<IIntermediateExpression> Arguments => _arguments;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left MethodCallExpression`</param>
        /// <param name="right">Right MethodCallExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(MethodCallExpression? left, MethodCallExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left MethodCallExpression</param>
        /// <param name="right">Right MethodCallExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(MethodCallExpression? left, MethodCallExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Name, Arguments);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(MethodCallExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(MethodCallExpression other)
        {
            return ItemType == other.ItemType
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Arguments.Equals(other.Arguments);
        }

        #endregion

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplyInternal(expression);
        }

        private void ApplyInternal(IIntermediateExpression expression)
        {
            _arguments.Add(expression);
        }
    }
}