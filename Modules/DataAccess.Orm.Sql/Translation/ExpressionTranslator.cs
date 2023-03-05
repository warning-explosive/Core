namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Concurrent;
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
        private readonly ISqlExpressionTranslatorComposite _translator;
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        private readonly ConcurrentDictionary<string, TranslatedSqlExpression> _cache;
        private readonly Func<string, Expression, TranslatedSqlExpression> _factory;

        public ExpressionTranslator(
            IModelProvider modelProvider,
            ILinqExpressionPreprocessorComposite preprocessor,
            ISqlExpressionTranslatorComposite translator,
            IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _modelProvider = modelProvider;
            _preprocessor = preprocessor;
            _translator = translator;
            _sqlFunctionProviders = sqlFunctionProviders;

            _cache = new ConcurrentDictionary<string, TranslatedSqlExpression>(StringComparer.Ordinal);
            _factory = TranslateSafe();
        }

        public ICommand Translate(Expression expression)
        {
            // TODO: #backlog - handle repository update/delete cache misses
            var translatedSqlExpression = ExtractExpressionCacheKeyExpressionVisitor.TryGetCacheKey(expression, out var cacheKey)
                ? _cache.GetOrAdd(cacheKey, _factory, expression)
                : _factory(string.Empty, expression);

            return new SqlCommand(
                translatedSqlExpression.CommandText,
                translatedSqlExpression.CommandParametersExtractor(expression));
        }

        private Func<string, Expression, TranslatedSqlExpression> TranslateSafe()
        {
            return (_, expression) => ExecutionExtensions
                .Try(TranslateUnsafe, expression)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));
        }

        private TranslatedSqlExpression TranslateUnsafe(Expression expression)
        {
            var sqlExpression = TranslationExpressionVisitor.Translate(
                new TranslationContext(),
                _modelProvider,
                _preprocessor,
                _sqlFunctionProviders,
                expression);

            return new TranslatedSqlExpression(
                sqlExpression.Expression,
                _translator.Translate(sqlExpression.Expression, 0),
                sqlExpression.CommandParametersExtractor);
        }
    }
}