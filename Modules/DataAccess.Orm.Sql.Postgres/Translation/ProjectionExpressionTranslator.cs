namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ProjectionExpressionTranslator : ISqlExpressionTranslator<ProjectionExpression>,
                                                    IResolvable<ISqlExpressionTranslator<ProjectionExpression>>,
                                                    ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ProjectionExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ProjectionExpression projectionExpression
                ? Translate(projectionExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ProjectionExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.IsDistinct ? "SELECT DISTINCT" : "SELECT");

            if (expression.Expressions.Any())
            {
                sb.AppendLine(expression
                    .Expressions
                    .Select(column => new string('\t', depth + 1) + _translator.Translate(column, depth))
                    .ToString("," + Environment.NewLine));
            }

            sb.Append(new string('\t', depth));
            sb.AppendLine("FROM");
            sb.Append(new string('\t', depth + 1));
            sb.Append(_translator.Translate(expression.Source, depth + 1));

            return sb.ToString();
        }
    }
}