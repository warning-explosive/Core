namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    public class QueryParameterExpression : IIntermediateExpression,
                                            IEquatable<QueryParameterExpression>,
                                            ISafelyEquatable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        private QueryParameterExpression(Type type, string name, object? value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Query parameter name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Value
        /// </summary>
        public object? Value { get; }

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
                Name.GetHashCode(StringComparison.OrdinalIgnoreCase),
                Value);
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
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
                   && Value?.Equals(other.Value) == true;
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return Expression.Constant(Value, Type);
        }

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="context">Translation context</param>
        /// <param name="type">Type</param>
        /// <param name="value">Value</param>
        /// <param name="force">Force query parameter</param>
        /// <returns>IIntermediateExpression</returns>
        public static IIntermediateExpression Create(
            TranslationContext context,
            Type type,
            object? value,
            bool force = false)
        {
            // see BinaryExpressionTranslator.IsNullConstant -> SQL uses "IS" and "IS NOT" operators with NULL-value
            var isNullConstant = value == null
                                 && value == type.DefaultValue()
                                 && !force;

            return isNullConstant
                ? new ConstantExpression(type, value)
                : new QueryParameterExpression(type, context.NextQueryParameterName(), value);
        }
    }
}