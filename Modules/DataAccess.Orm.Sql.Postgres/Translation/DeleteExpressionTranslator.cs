namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class DeleteExpressionTranslator : ISqlExpressionTranslator<DeleteExpression>,
                                                IResolvable<ISqlExpressionTranslator<DeleteExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;

        public DeleteExpressionTranslator(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is DeleteExpression deleteExpression
                ? Translate(deleteExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(DeleteExpression expression, int depth)
        {
            var table = _modelProvider.Tables[expression.Type];

            return $@"DELETE FROM ""{table.Schema}"".""{table.Name}""";
        }
    }
}