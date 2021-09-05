namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Exceptions;

    [Component(EnLifestyle.Scoped)]
    internal class ExpressionTranslator : IExpressionTranslator
    {
        private readonly IEnumerable<IMemberInfoTranslator> _sqlFunctionProviders;

        public ExpressionTranslator(IEnumerable<IMemberInfoTranslator> sqlFunctionProviders)
        {
            _sqlFunctionProviders = sqlFunctionProviders;
        }

        public IIntermediateExpression Translate(Expression expression)
        {
            var visitor = new TranslationExpressionVisitor(this, _sqlFunctionProviders);

            ExecutionExtensions
                .Try(expression, visitor.Visit)
                .Catch<Exception>()
                .Invoke(ex => throw new TranslationException(expression, ex));

            return visitor
                .Context
                .Expression
                .EnsureNotNull(() => new TranslationException(expression));
        }
    }
}