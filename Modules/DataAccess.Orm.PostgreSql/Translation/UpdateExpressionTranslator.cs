namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class UpdateExpressionTranslator : ISqlExpressionTranslator<UpdateExpression>,
                                                IResolvable<ISqlExpressionTranslator<UpdateExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;

        public UpdateExpressionTranslator(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is UpdateExpression updateExpression
                ? Translate(updateExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(UpdateExpression expression, int depth)
        {
            var table = _modelProvider.Tables[expression.Type];

            return $@"UPDATE ""{table.Schema}"".""{table.Name}""";
        }
    }
}