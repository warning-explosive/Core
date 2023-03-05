namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// QueryParameterExpression
    /// </summary>
    public class QueryParameterExpression : ITypedSqlExpression
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
            var name = context.NextCommandParameterName();

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
    }
}