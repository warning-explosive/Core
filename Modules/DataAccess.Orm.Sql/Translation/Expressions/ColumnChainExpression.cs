namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// ColumnChainExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ColumnChainExpression : IIntermediateExpression,
                                         IEquatable<ColumnChainExpression>,
                                         ISafelyEquatable<ColumnChainExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="chain">Bindings chain</param>
        /// <param name="expression">IIntermediateExpression</param>
        public ColumnChainExpression(
            IReadOnlyCollection<SimpleBindingExpression> chain,
            IIntermediateExpression expression)
        {
            Type = chain.Last().Type;
            Chain = chain;
            Source = expression;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Chain
        /// </summary>
        public IReadOnlyCollection<SimpleBindingExpression> Chain { get; }

        /// <summary>
        /// Source
        /// </summary>
        public IIntermediateExpression Source { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ColumnChainExpression</param>
        /// <param name="right">Right ColumnChainExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ColumnChainExpression? left, ColumnChainExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ColumnChainExpression</param>
        /// <param name="right">Right ColumnChainExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ColumnChainExpression? left, ColumnChainExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return new object[] { Source }
                .Concat(Chain)
                .Aggregate(Type.GetHashCode(), HashCode.Combine);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(ColumnChainExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ColumnChainExpression other)
        {
            return Type == other.Type
                   && Source.Equals(other.Source)
                   && Chain.SequenceEqual(other.Chain);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(ColumnChainExpression) + "." + nameof(AsExpressionTree));
        }
    }
}