namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Api.Exceptions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Model;

    [Component(EnLifestyle.Scoped)]
    internal class ExpressionTranslator : IExpressionTranslator
    {
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        private readonly ExpressionVisitor[] _expressionVisitors;
        private readonly IIntermediateExpressionVisitor[] _intermediateExpressionVisitors;

        public ExpressionTranslator(
            IModelProvider modelProvider,
            IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _sqlFunctionProviders = sqlFunctionProviders;

            _expressionVisitors = new ExpressionVisitor[]
            {
                new CollapseConstantsExpressionVisitor()
            };

            _intermediateExpressionVisitors = new IIntermediateExpressionVisitor[]
            {
                new JoinColumnChainsVisitor(modelProvider)
            };
        }

        public IIntermediateExpression Translate(Expression expression)
        {
            expression = ExecutionExtensions
                .Try(expression, Aggregate)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            var visitor = new TranslationExpressionVisitor(this, _sqlFunctionProviders);

            ExecutionExtensions
                .Try(expression, visitor.Visit)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            var intermediateExpression = visitor
                .Context
                .Expression
                .EnsureNotNull(() => new TranslationException(expression));

            return ExecutionExtensions
                .Try(intermediateExpression, Aggregate)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));
        }

        private Expression Aggregate(Expression expression)
        {
            return _expressionVisitors.Aggregate(expression, (acc, next) => next.Visit(acc));
        }

        private IIntermediateExpression Aggregate(IIntermediateExpression expression)
        {
            return _intermediateExpressionVisitors.Aggregate(expression, (acc, next) => next.Visit(acc));
        }
    }
}