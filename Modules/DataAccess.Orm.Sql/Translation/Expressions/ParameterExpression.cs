namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using Basics;

    /// <summary>
    /// ParameterExpression
    /// </summary>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class ParameterExpression : IIntermediateExpression,
                                       IEquatable<ParameterExpression>,
                                       ISafelyEquatable<ParameterExpression>
    {
        private readonly Func<string> _nameProducer;

        /// <summary> .cctor </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="type">Type</param>
        public ParameterExpression(TranslationContext context, Type type)
        {
            Type = type;
            _nameProducer = context.NextLambdaParameterName();
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => _nameProducer();

        #region IEquatable

        /// <summary>
        /// operator ==
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>equals</returns>
        public static bool operator ==(ParameterExpression? left, ParameterExpression? right)
        {
            return Equatable.Equals(left, right);
        }

        /// <summary>
        /// operator !=
        /// </summary>
        /// <param name="left">Left ParameterExpression</param>
        /// <param name="right">Right ParameterExpression</param>
        /// <returns>not equals</returns>
        public static bool operator !=(ParameterExpression? left, ParameterExpression? right)
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
        public bool Equals(ParameterExpression? other)
        {
            return Equatable.Equals(this, other);
        }

        /// <inheritdoc />
        public bool SafeEquals(ParameterExpression other)
        {
            return Type == other.Type
                   && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <inheritdoc />
        public Expression AsExpressionTree()
        {
            return Expression.Parameter(Type, Name);
        }
    }
}