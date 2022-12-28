namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using Basics;

    /// <summary>
    /// RowsFetchLimitExpression
    /// </summary>
    public class RowsFetchLimitExpression : ISqlExpression,
                                            IEquatable<RowsFetchLimitExpression>,
                                            ISafelyEquatable<RowsFetchLimitExpression>,
                                            IApplicable<ProjectionExpression>,
                                            IApplicable<FilterExpression>,
                                            IApplicable<JoinExpression>,
                                            IApplicable<NamedSourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="rowsFetchLimit">Rows fetch limit</param>
        /// <param name="source">Source expression</param>
        public RowsFetchLimitExpression(
            Type type,
            uint rowsFetchLimit,
            ISqlExpression source)
        {
            Type = type;
            RowsFetchLimit = rowsFetchLimit;
            Source = source;
        }

        internal RowsFetchLimitExpression(Type type, uint rowsFetchLimit)
            : this(type, rowsFetchLimit, null!)
        {
            Type = type;
            RowsFetchLimit = rowsFetchLimit;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Rows fetch limit
        /// </summary>
        public uint RowsFetchLimit { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left RowsFetchLimitExpression</param>
        /// <param name="right">Right RowsFetchLimitExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(RowsFetchLimitExpression? left, RowsFetchLimitExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left RowsFetchLimitExpression</param>
        /// <param name="right">Right RowsFetchLimitExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(RowsFetchLimitExpression? left, RowsFetchLimitExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, RowsFetchLimit, Source);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(RowsFetchLimitExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(RowsFetchLimitExpression other)
        {
            return Type == other.Type
                   && RowsFetchLimit == other.RowsFetchLimit
                   && Source.Equals(other.Source);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            throw new TranslationException(nameof(RowsFetchLimitExpression) + "." + nameof(AsExpressionTree));
        }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
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

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Rows fetch limit expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}