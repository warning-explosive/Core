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

    [Component(EnLifestyle.Scoped)]
    internal class ExpressionTranslator : IExpressionTranslator
    {
        private static readonly ExpressionVisitor[] Visitors = new ExpressionVisitor[]
        {
            new CollapseConstantsExpressionVisitor()
        };

        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        public ExpressionTranslator(IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _sqlFunctionProviders = sqlFunctionProviders;
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

            return visitor
                .Context
                .Expression
                .EnsureNotNull(() => new TranslationException(expression));

            static Expression Aggregate(Expression expression)
            {
                return Visitors.Aggregate(expression, (acc, next) => next.Visit(acc));
            }
        }
    }
}