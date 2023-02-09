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
    using CompositionRoot;
    using Linq;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class ExpressionTranslator : IQueryTranslator,
                                          IResolvable<IQueryTranslator>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IModelProvider _modelProvider;
        private readonly ILinqExpressionPreprocessorComposite _preprocessor;
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        public ExpressionTranslator(
            IDependencyContainer dependencyContainer,
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            ISqlExpressionTranslatorComposite sqlExpressionTranslator,
            IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _dependencyContainer = dependencyContainer;
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _sqlExpressionTranslator = sqlExpressionTranslator;
            _sqlFunctionProviders = sqlFunctionProviders;
        }

        public IQuery Translate(Expression expression)
        {
            var translatedSqlQuery = ExecutionExtensions
                .Try(expression, TranslateUnsafe)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            return new FlatQuery(
                translatedSqlQuery.CommandText,
                translatedSqlQuery.CommandParametersExtractor(expression));
        }

        private TranslatedSqlQuery TranslateUnsafe(Expression expression)
        {
            var visitor = new TranslationExpressionVisitor(new TranslationContext(), _dependencyContainer, _modelProvider, _preprocessor, _sqlFunctionProviders);

            var sqlQuery = visitor.Translate(_preprocessor.Visit(expression));

            var commandText = _sqlExpressionTranslator.Translate(sqlQuery.SqlExpression, 0);

            return new TranslatedSqlQuery(sqlQuery.SqlExpression, commandText, sqlQuery.CommandParametersExtractor);
        }
    }
}