namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Text;
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
        private readonly ISqlExpressionTranslatorComposite _translator;

        public DeleteExpressionTranslator(
            IModelProvider modelProvider,
            ISqlExpressionTranslatorComposite translator)
        {
            _modelProvider = modelProvider;
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is DeleteExpression deleteExpression
                ? Translate(deleteExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(DeleteExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var table = _modelProvider.Tables[expression.ItemType];

            sb.Append(new string('\t', depth));

            if (expression.FilterExpression != null)
            {
                sb.AppendLine($@"DELETE FROM ""{table.Schema}"".""{table.Name}""");
                sb.Append(_translator.Translate(expression.FilterExpression, depth));
            }
            else
            {
                sb.Append($@"DELETE FROM ""{table.Schema}"".""{table.Name}""");
            }

            return sb.ToString();
        }
    }
}