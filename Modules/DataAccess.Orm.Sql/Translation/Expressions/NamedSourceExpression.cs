namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    public class NamedSourceExpression : ISqlExpression,
                                         IEquatable<NamedSourceExpression>,
                                         ISafelyEquatable<NamedSourceExpression>,
                                         IApplicable<FilterExpression>,
                                         IApplicable<ProjectionExpression>,
                                         IApplicable<OrderByExpression>,
                                         IApplicable<QuerySourceExpression>,
                                         IApplicable<NewExpression>,
                                         IApplicable<SimpleBindingExpression>,
                                         IApplicable<NamedBindingExpression>,
                                         IApplicable<BinaryExpression>,
                                         IApplicable<ConditionalExpression>,
                                         IApplicable<MethodCallExpression>,
                                         IApplicable<ParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type type,
            ISqlExpression source,
            ISqlExpression parameter)
        {
            Type = type;
            Source = source;
            Parameter = parameter;
        }

        internal NamedSourceExpression(Type type, TranslationContext context)
            : this(type, null!, context.NextParameterExpression(type))
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Parameter expression
        /// </summary>
        public ISqlExpression Parameter { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left NamedSourceExpression</param>
        /// <param name="right">Right NamedSourceExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(NamedSourceExpression? left, NamedSourceExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left NamedSourceExpression</param>
        /// <param name="right">Right NamedSourceExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(NamedSourceExpression? left, NamedSourceExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Parameter);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(NamedSourceExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(NamedSourceExpression other)
        {
            return Type == other.Type
                   && Source.Equals(other.Source)
                   && Parameter.Equals(other.Parameter);
        }

        #endregion

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, OrderByExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NewExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedBindingExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ForwardExpression(context, expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Named source expression source has already been set");
            }

            Source = expression;
        }

        private void ForwardExpression(TranslationContext context, ISqlExpression expression)
        {
            context.Apply(Source is FilterExpression filterExpression ? filterExpression.Source : Source, expression);
        }

        #endregion
    }
}