namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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
                .Try(() => visitor.Visit(expression))
                .Catch<Exception>()
                .Invoke(ex => new TranslationException(expression, ex).Rethrow());

            return visitor
                .Context
                .Expression
                .EnsureNotNull(() => new TranslationException(expression, new NullReferenceException(nameof(TranslationContext.Expression))));
        }
    }
}