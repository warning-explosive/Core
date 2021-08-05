namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions;
    using Basics;
    using Internals;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ProjectionExpression : ISubsequentIntermediateExpression,
                                        IEquatable<ProjectionExpression>,
                                        ISafelyEquatable<ProjectionExpression>,
                                        IApplicable<ProjectionExpression>,
                                        IApplicable<FilterExpression>,
                                        IApplicable<QuerySourceExpression>,
                                        IApplicable<NamedSourceExpression>,
                                        IApplicable<NewExpression>,
                                        IApplicable<SimpleBindingExpression>,
                                        IApplicable<NamedBindingExpression>,
                                        IApplicable<BinaryExpression>,
                                        IApplicable<ConditionalExpression>,
                                        IApplicable<MethodCallExpression>,
                                        IApplicable<ParameterExpression>
    {
        private List<IIntermediateExpression> _bindings;

        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="bindings">Bindings</param>
        public ProjectionExpression(
            Type type,
            IIntermediateExpression source,
            IEnumerable<IIntermediateExpression> bindings)
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
            : this(type, null !, new List<IIntermediateExpression>())
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
        /// Transformation bindings
        /// </summary>
        public IReadOnlyCollection<IIntermediateExpression> Bindings => _bindings;

        /// <summary>
        /// Source expression which we want to transform
        /// </summary>
        public IIntermediateExpression Source { get; private set; } = null!;

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
            return HashCode.Combine(Type, IsProjectionToClass, IsAnonymousProjection, IsDistinct, Bindings, Source);
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
                   && Bindings.SequenceEqual(other.Bindings)
                   && Source.Equals(other.Source);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new NotImplementedException(nameof(ProjectionExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, NewExpression @new)
        {
            IsProjectionToClass = true;
            IsAnonymousProjection = @new.Type.IsAnonymous();
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SimpleBindingExpression binding)
        {
            _bindings.Add(binding);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedBindingExpression binding)
        {
            _bindings.Add(binding);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression binary)
        {
            _bindings.Add(binary);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression conditional)
        {
            _bindings.Add(conditional);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression methodCall)
        {
            _bindings.Add(methodCall);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression parameter)
        {
            if (Source is not NamedSourceExpression)
            {
                Source = new NamedSourceExpression(Source.Type, Source, parameter);
            }
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression projection)
        {
            ApplySource(context, projection);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression filter)
        {
            ApplySource(context, filter);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression querySource)
        {
            ApplySource(context, querySource);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression namedSource)
        {
            ApplySource(context, namedSource);
        }

        private void ApplySource(TranslationContext context, IIntermediateExpression expression)
        {
            Source = Source is not NamedSourceExpression
                     && expression is not NamedSourceExpression
                ? new NamedSourceExpression(expression.Type, expression, context.GetParameterExpression(expression.Type))
                : expression;
        }

        #endregion
    }
}