namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class UpdateExpressionTranslator : ISqlExpressionTranslator<UpdateExpression>,
                                                IResolvable<ISqlExpressionTranslator<UpdateExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;
        private readonly ISqlExpressionTranslatorComposite _translator;

        public UpdateExpressionTranslator(
            IModelProvider modelProvider,
            ISqlExpressionTranslatorComposite translator)
        {
            _modelProvider = modelProvider;
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is UpdateExpression updateExpression
                ? Translate(updateExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(UpdateExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var table = _modelProvider.Tables[expression.ItemType];

            sb.Append(new string('\t', depth));
            sb.AppendLine($@"UPDATE ""{table.Schema}"".""{table.Name}""");

            sb.Append(new string('\t', depth));
            sb.Append("SET ");

            var assignments = expression
                .Assignments
                .Select(assignment => new string('\t', depth) + _translator.Translate(assignment, depth))
                .ToString(", ");

            if (expression.FilterExpression != null)
            {
                sb.AppendLine(assignments);
                sb.Append(_translator.Translate(expression.FilterExpression, depth));
            }
            else
            {
                sb.Append(assignments);
            }

            return sb.ToString();
        }
    }
}