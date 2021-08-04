namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Abstractions;
    using Basics;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class QueryParameterExpression : IIntermediateExpression,
                                            IEquatable<QueryParameterExpression>,
                                            ISafelyEquatable<QueryParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Query parameter name</param>
        /// <param name="value">Query parameter value</param>
        /// <param name="queryParameterBinding">Query parameter binding</param>
        private QueryParameterExpression(
            Type type,
            string name,
            object? value,
            INamedIntermediateExpression? queryParameterBinding = null)
        {
            Type = type;
            Name = name;
            Value = value;
            QueryParameterBinding = queryParameterBinding;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Query parameter name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Query parameter value
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Query parameter binding
        /// </summary>
        public INamedIntermediateExpression? QueryParameterBinding { get; }

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
            return HashCode.Combine(Type, Name, Value, QueryParameterBinding);
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
                   && Value?.Equals(other.Value) == true
                   && QueryParameterBinding.Equals(other.QueryParameterBinding);
        }

        #endregion

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="context">Translation context</param>
        /// <param name="type">Type</param>
        /// <param name="value">Value</param>
        /// <param name="queryParameterBinding">QueryParameterBinding</param>
        /// <returns>IIntermediateExpression</returns>
        public static IIntermediateExpression Create(
            TranslationContext context,
            Type type,
            object? value,
            INamedIntermediateExpression? queryParameterBinding = null)
        {
            var isNullValueParameter = value == null
                                       && value == type.DefaultValue()
                                       && queryParameterBinding == null;

            return isNullValueParameter
                ? new ConstantExpression(type, value)
                : new QueryParameterExpression(type, context.NextQueryParameterName(), value, queryParameterBinding);
        }
    }
}