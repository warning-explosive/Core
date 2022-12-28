namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq;

    [Component(EnLifestyle.Singleton)]
    internal class QueryTranslator : IQueryTranslator,
                                     IResolvable<IQueryTranslator>
    {
        private readonly IExpressionTranslator _translator;
        private readonly ISqlQueryTranslator _sqlTranslator;

        public QueryTranslator(
            IExpressionTranslator translator,
            ISqlQueryTranslator sqlTranslator)
        {
            _translator = translator;
            _sqlTranslator = sqlTranslator;
        }

        public IQuery Translate(Expression expression)
        {
            return _sqlTranslator.Translate(_translator.Translate(expression));
        }
    }
}