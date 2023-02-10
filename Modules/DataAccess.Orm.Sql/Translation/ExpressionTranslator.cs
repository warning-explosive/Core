namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Model;
    using Orm.Linq;

    [Component(EnLifestyle.Singleton)]
    internal class ExpressionTranslator : IExpressionTranslator,
                                          IResolvable<IExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        public ExpressionTranslator(
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            ISqlExpressionTranslatorComposite sqlExpressionTranslator,
            IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _sqlExpressionTranslator = sqlExpressionTranslator;
            _sqlFunctionProviders = sqlFunctionProviders;
        }

        public ICommand Translate(Expression expression)
        {
            // TODO: add cache
            var translatedSqlQuery = ExecutionExtensions
                .Try(expression, TranslateUnsafe)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            return new SqlCommand(
                translatedSqlQuery.CommandText,
                translatedSqlQuery.CommandParametersExtractor(expression));
        }

        private TranslatedSqlExpression TranslateUnsafe(Expression expression)
        {
            var visitor = new TranslationExpressionVisitor(new TranslationContext(), _modelProvider, _preprocessor, _sqlFunctionProviders);

            var sqlExpression = visitor.Translate(_preprocessor.Visit(expression));

            return new TranslatedSqlExpression(
                sqlExpression.Expression,
                _sqlExpressionTranslator.Translate(sqlExpression.Expression, 0),
                sqlExpression.CommandParametersExtractor);
        }
    }
}