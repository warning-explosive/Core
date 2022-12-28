namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ProjectionExpressionTranslator : ISqlExpressionTranslator<ProjectionExpression>,
                                                    IResolvable<ISqlExpressionTranslator<ProjectionExpression>>,
                                                    ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public ProjectionExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
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

            if (expression.Bindings.Any())
            {
                var lastBindingIndex = expression.Bindings.Count - 1;

                expression
                    .Bindings
                    .Select(binding => _sqlExpressionTranslator.Translate(binding, depth))
                    .Each((binding, i) =>
                    {
                        sb.Append(new string('\t', depth + 1));
                        sb.Append(binding);
                        var ending = i < lastBindingIndex
                            ? ","
                            : string.Empty;

                        sb.AppendLine(ending);
                    });
            }

            sb.Append(new string('\t', depth));
            sb.AppendLine("FROM");
            sb.Append(new string('\t', depth + 1));
            sb.Append(_sqlExpressionTranslator.Translate(expression.Source, depth + 1));

            return sb.ToString();
        }
    }
}