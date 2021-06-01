namespace SpaceEngineers.Core.DataAccess.Orm.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Abstractions;
    using Basics;
    using Linq;

    /// <summary>
    /// ProjectionExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ProjectionExpression : ISubsequentIntermediateExpression,
                                        IEquatable<ProjectionExpression>,
                                        ISafelyEquatable<ProjectionExpression>
    {
        private List<IIntermediateExpression> _bindings;

        /// <summary> .cctor </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="source">Source</param>
        /// <param name="bindings">Bindings</param>
        public ProjectionExpression(
            Type itemType,
            IIntermediateExpression source,
            IEnumerable<IIntermediateExpression> bindings)
        {
            ItemType = itemType;
            Source = source;
            IsProjectionToClass = itemType.IsClass;
            IsAnonymousProjection = itemType.IsAnonymous();

            _bindings = bindings.ToList();
        }

        internal ProjectionExpression(Type itemType)
        {
            ItemType = itemType;
            _bindings = new List<IIntermediateExpression>();
        }

        /// <inheritdoc />
        public Type ItemType { get; }

        /// <summary>
        /// Is projection creates anonymous or user defined class
        /// </summary>
        public bool IsProjectionToClass { get; private set; }

        /// <summary>
        /// Is projection creates anonymous class
        /// </summary>
        public bool IsAnonymousProjection { get; private set; }

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
            return HashCode.Combine(ItemType, IsProjectionToClass, IsAnonymousProjection, Bindings, Source);
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
            return ItemType == other.ItemType
                   && IsProjectionToClass == other.IsProjectionToClass
                   && IsAnonymousProjection == other.IsAnonymousProjection
                   && Bindings.SequenceEqual(other.Bindings)
                   && Source.Equals(other.Source);
        }

        #endregion

        internal void Apply(QuerySourceExpression querySource)
        {
            Source = querySource;
        }

        internal void Apply(ProjectionExpression projection)
        {
            Source = projection;
        }

        internal void Apply(FilterExpression filter)
        {
            Source = filter;
        }

        internal void Apply(NewExpression @new)
        {
            IsProjectionToClass = true;
            IsAnonymousProjection = @new.ItemType.IsAnonymous();
        }

        internal void Apply(SimpleBindingExpression binding)
        {
            _bindings.Add(binding);
        }

        internal void Apply(NamedBindingExpression binding)
        {
            _bindings.Add(binding);
        }

        internal void Apply(ConditionalExpression conditional)
        {
            _bindings.Add(conditional);
        }

        internal void Apply(BinaryExpression binary)
        {
            _bindings.Add(binary);
        }

        internal void Apply(MethodCallExpression methodCall)
        {
            _bindings.Add(methodCall);
        }

        internal void Apply(ParameterExpression parameter)
        {
            var visitor = new ReplaceParameterVisitor(parameter);

            if (IsProjectionToClass)
            {
                _bindings = Bindings
                    .Select(binding => visitor.Visit(binding))
                    .ToList();
            }

            if (Source is NamedSourceExpression namedSource)
            {
                Source = visitor.Visit(namedSource);
            }
            else
            {
                Source = new NamedSourceExpression(
                    Source.ItemType,
                    Source,
                    parameter);
            }
        }
    }
}