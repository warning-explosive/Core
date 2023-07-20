namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class BatchExpressionTranslator : ISqlExpressionTranslator<BatchExpression>,
                                               IResolvable<ISqlExpressionTranslator<BatchExpression>>,
                                               ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public BatchExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is BatchExpression batchExpression
                ? Translate(batchExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(BatchExpression expression, int depth)
        {
            return expression
                .Expressions
                .Select(innerExpression => _translator.Translate(innerExpression, depth))
                .ToString(";" + Environment.NewLine);
        }
    }
}