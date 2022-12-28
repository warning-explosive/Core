namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class ExpressionTranslator : IExpressionTranslator,
                                          IResolvable<IExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        private readonly ExpressionVisitor[] _expressionVisitors;
        private readonly ISqlExpressionVisitor[] _sqlExpressionVisitors;

        public ExpressionTranslator(
            IModelProvider modelProvider,
            IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _modelProvider = modelProvider;
            _sqlFunctionProviders = sqlFunctionProviders;

            _expressionVisitors = new ExpressionVisitor[]
            {
                new CollapseConstantsExpressionVisitor(),
                new UnwrapScalarQueryableMethodsWithPredicateExpressionVisitor()
            };

            _sqlExpressionVisitors = Array.Empty<ISqlExpressionVisitor>();
        }

        public ISqlExpression Translate(Expression expression)
        {
            expression = ExecutionExtensions
                .Try(expression, PreAggregate)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            var visitor = new TranslationExpressionVisitor(_modelProvider, this, _sqlFunctionProviders);

            ExecutionExtensions
                .Try(expression, visitor.Visit)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            var sqlExpression = visitor
                .Context
                .Expression
                .EnsureNotNull(() => new TranslationException(expression));

            return ExecutionExtensions
                .Try(sqlExpression, PostAggregate)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));
        }

        private Expression PreAggregate(Expression expression)
        {
            return _expressionVisitors.Aggregate(expression, (acc, next) => next.Visit(acc));
        }

        private ISqlExpression PostAggregate(ISqlExpression expression)
        {
            return _sqlExpressionVisitors.Aggregate(expression, (acc, next) => next.Visit(acc));
        }
    }
}