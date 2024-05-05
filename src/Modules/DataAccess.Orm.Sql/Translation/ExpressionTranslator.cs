namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Logging;
    using Exceptions;
    using Linq;
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Singleton)]
    internal class ExpressionTranslator : IExpressionTranslator,
                                          IResolvable<IExpressionTranslator>
    {
        private readonly TranslationExpressionVisitor _translationExpressionVisitor;
        private readonly ISqlExpressionTranslatorComposite _translator;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, TranslatedSqlExpression> _cache;
        private readonly Func<string, Expression, TranslatedSqlExpression> _factory;

        public ExpressionTranslator(
            TranslationExpressionVisitor translationExpressionVisitor,
            ISqlExpressionTranslatorComposite translator,
            ILogger logger)
        {
            _translationExpressionVisitor = translationExpressionVisitor;
            _translator = translator;
            _logger = logger;

            _cache = new ConcurrentDictionary<string, TranslatedSqlExpression>(StringComparer.Ordinal);
            _factory = TranslateSafe();
        }

        public ICommand Translate(Expression expression)
        {
            TranslatedSqlExpression translatedSqlExpression;

            if (ExtractExpressionCacheKeyExpressionVisitor.TryGetCacheKey(expression, out var cacheKey))
            {
                translatedSqlExpression = _cache.GetOrAdd(cacheKey, _factory, expression);
            }
            else
            {
                _logger.Warning($"{nameof(LinqExtensions.CachedExpression)} can be applied so as to eliminate repetitive translations");

                translatedSqlExpression = _factory(string.Empty, expression);
            }

            return new SqlCommand(
                translatedSqlExpression.CommandText,
                translatedSqlExpression.CommandParametersExtractor(expression));
        }

        private Func<string, Expression, TranslatedSqlExpression> TranslateSafe()
        {
            return (key, expression) =>
            {
                _logger.Information($"building cached expression: {key}");

                return ExecutionExtensions
                    .Try(TranslateUnsafe, expression)
                    .Catch<Exception>()
                    .Invoke(ex => throw new TranslationException(expression, ex));
            };
        }

        private TranslatedSqlExpression TranslateUnsafe(Expression expression)
        {
            var sqlExpression = _translationExpressionVisitor.Translate(new TranslationContext(), expression);

            return new TranslatedSqlExpression(
                sqlExpression.Expression,
                _translator.Translate(sqlExpression.Expression, 0),
                sqlExpression.CommandParametersExtractor);
        }
    }
}