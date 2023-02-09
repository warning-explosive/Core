namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    /// <summary>
    /// MethodCallExpression
    /// </summary>
    public class MethodCallExpression : ISqlExpression,
                                        IEquatable<MethodCallExpression>,
                                        ISafelyEquatable<MethodCallExpression>,
                                        IApplicable<SimpleBindingExpression>,
                                        IApplicable<ConditionalExpression>
    {
        private readonly List<ISqlExpression> _arguments;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="source">Source</param>
        /// <param name="arguments">Arguments</param>
        public MethodCallExpression(
            Type type,
            string name,
            ISqlExpression? source,
            IEnumerable<ISqlExpression> arguments)
        {
            Type = type;
            Name = name;
            Source = source;
            _arguments = arguments.ToList();
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression? Source { get; }

        /// <summary>
        /// Expression
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Arguments => _arguments;

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
            return HashCode.Combine(
                Type,
                Name.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Source,
                Arguments);
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
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Source == other.Source
                   && Arguments.SequenceEqual(other.Arguments);
        }

        #endregion

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplyInternal(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyInternal(expression);
        }

        private void ApplyInternal(ISqlExpression expression)
        {
            _arguments.Add(expression);
        }

        #endregion
    }
}