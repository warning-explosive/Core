namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    public class QueryParameterExpression : ISqlExpression,
                                            IEquatable<QueryParameterExpression>,
                                            ISafelyEquatable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="type">Type</param>
        /// <param name="extractor">Extractor</param>
        public QueryParameterExpression(
            TranslationContext context,
            Type type,
            Func<Expression, ConstantExpression>? extractor = null)
        {
            var name = context.NextQueryParameterName();

            Type = type;
            Name = name;

            context.CaptureCommandParameterExtractor(name, extractor);
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Query parameter name
        /// </summary>
        public string Name { get; }

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left QueryParameterExpression</param>
        /// <param name="right">Right QueryParameterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(QueryParameterExpression? left, QueryParameterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left QueryParameterExpression</param>
        /// <param name="right">Right QueryParameterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(QueryParameterExpression? left, QueryParameterExpression? right)
        {
            return !Equatable.Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Type,
                Name.GetHashCode(StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return Equatable.Equals(this, obj);
        }

        /// <inheritdoc />
        public bool Equals(QueryParameterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(QueryParameterExpression other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}