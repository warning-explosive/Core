namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    public class ProjectionExpression : ISqlExpression,
                                        IEquatable<ProjectionExpression>,
                                        ISafelyEquatable<ProjectionExpression>,
                                        IApplicable<FilterExpression>,
                                        IApplicable<JoinExpression>,
                                        IApplicable<NamedSourceExpression>,
                                        IApplicable<NewExpression>,
                                        IApplicable<SimpleBindingExpression>,
                                        IApplicable<NamedBindingExpression>,
                                        IApplicable<BinaryExpression>,
                                        IApplicable<UnaryExpression>,
                                        IApplicable<ConditionalExpression>,
                                        IApplicable<MethodCallExpression>
    {
        private readonly List<ISqlExpression> _bindings;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source expression</param>
        /// <param name="bindings">Bindings expressions</param>
        public ProjectionExpression(
            Type type,
            ISqlExpression source,
            IEnumerable<ISqlExpression> bindings)
        {
            Type = type;
            Source = source;
            IsProjectionToClass = type.IsClass
                                  && !type.IsPrimitive()
                                  && !type.IsCollection();
            IsAnonymousProjection = type.IsAnonymous();

            _bindings = bindings.ToList();
        }

        internal ProjectionExpression(Type type)
            : this(type, null!, Array.Empty<ISqlExpression>())
        {
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Is projection creates anonymous or user defined class
        /// </summary>
        public bool IsProjectionToClass { get; private set; }

        /// <summary>
        /// Is projection creates anonymous class
        /// </summary>
        public bool IsAnonymousProjection { get; private set; }

        /// <summary>
        /// Is projection takes distinct values
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Bindings expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Bindings => _bindings;

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ProjectionExpression</param>
        /// <param name="right">Right ProjectionExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ProjectionExpression? left, ProjectionExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ProjectionExpression</param>
        /// <param name="right">Right ProjectionExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ProjectionExpression? left, ProjectionExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, IsProjectionToClass, IsAnonymousProjection, IsDistinct, Source, Bindings);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ProjectionExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ProjectionExpression other)
        {
            return Type == other.Type
                   && IsProjectionToClass == other.IsProjectionToClass
                   && IsAnonymousProjection == other.IsAnonymousProjection
                   && IsDistinct == other.IsDistinct
                   && Source.Equals(other.Source)
                   && Bindings.SequenceEqual(other.Bindings);
        }

        #endregion

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, NewExpression expression)
        {
            IsProjectionToClass = true;
            IsAnonymousProjection = expression.Type.IsAnonymous();
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedBindingExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ApplyBinding(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JoinExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplyBinding(ISqlExpression expression)
        {
            if (Source is JoinExpression join)
            {
                expression = expression.ReplaceJoinBindings(join, true);
            }

            if (expression is ParameterExpression)
            {
                return;
            }

            _bindings.Add(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Projection expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}