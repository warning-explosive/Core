namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// UnaryExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class UnaryExpression : IIntermediateExpression,
                                   IEquatable<UnaryExpression>,
                                   ISafelyEquatable<UnaryExpression>,
                                   IApplicable<SimpleBindingExpression>,
                                   IApplicable<ConditionalExpression>,
                                   IApplicable<BinaryExpression>,
                                   IApplicable<UnaryExpression>,
                                   IApplicable<ParameterExpression>,
                                   IApplicable<QueryParameterExpression>,
                                   IApplicable<ConstantExpression>,
                                   IApplicable<SpecialExpression>,
                                   IApplicable<MethodCallExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="source">Source expression</param>
        public UnaryExpression(
            Type type,
            UnaryOperator @operator,
            IIntermediateExpression source)
        {
            Type = type;
            Operator = @operator;
            Source = source;
        }

        internal UnaryExpression(Type type, UnaryOperator @operator)
            : this(type, @operator, null!)
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public UnaryOperator Operator { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public IIntermediateExpression Source { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left UnaryExpression</param>
        /// <param name="right">Right UnaryExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(UnaryExpression? left, UnaryExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left UnaryExpression</param>
        /// <param name="right">Right UnaryExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(UnaryExpression? left, UnaryExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Operator, Source);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(UnaryExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(UnaryExpression other)
        {
            return Type == other.Type
                   && Operator == other.Operator
                   && Source.Equals(other.Source);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return System.Linq.Expressions.Expression.MakeUnary(Operator.AsExpressionType(), Source.AsExpressionTree(), Type);
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConstantExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SpecialExpression expression)
        {
            ApplySource(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ApplySource(context, expression);
        }

        private void ApplySource(TranslationContext context, IIntermediateExpression expression)
        {
            if (Source == null)
            {
                Source = expression;
                return;
            }

            throw new InvalidOperationException("Unary expression source have already been set");
        }

        #endregion
    }
}