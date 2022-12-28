namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Expressions;
    using Extensions;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class SqlQueryTranslator : ISqlQueryTranslator,
                                        IResolvable<ISqlQueryTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public SqlQueryTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslator)
        {
            _sqlExpressionTranslator = sqlExpressionTranslator;
        }

        public IQuery Translate(ISqlExpression expression)
        {
            return expression switch
            {
                GroupByExpression groupByExpression => TranslateGroupByExpression(groupByExpression),
                _ => TranslateSqlExpression(expression)
            };
        }

        private GroupedQuery TranslateGroupByExpression(GroupByExpression expression)
        {
            var keysQuery = _sqlExpressionTranslator.Translate(expression.KeysExpression, 0);
            var keysQueryParameters = expression.KeysExpression.ExtractQueryParameters();

            return new GroupedQuery(keysQuery, keysQueryParameters, expression.ValuesExpressionProducer);
        }

        private FlatQuery TranslateSqlExpression(ISqlExpression expression)
        {
            var query = _sqlExpressionTranslator.Translate(expression, 0);
            var queryParameters = expression.ExtractQueryParameters();

            return new FlatQuery(query, queryParameters);
        }
    }
}