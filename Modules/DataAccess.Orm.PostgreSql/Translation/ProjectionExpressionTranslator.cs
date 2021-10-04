namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class ProjectionExpressionTranslator : IExpressionTranslator<ProjectionExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ProjectionExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(ProjectionExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.IsDistinct ? "SELECT DISTINCT" : "SELECT");

            var lastBindingIndex = expression.Bindings.Count - 1;

            if (expression.Bindings.Any())
            {
                expression
                    .Bindings
                    .Select(binding => binding.Translate(_dependencyContainer, depth))
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
            else
            {
                sb.Append(new string('\t', depth + 1));
                sb.AppendLine("*");
            }

            sb.Append(new string('\t', depth));
            sb.AppendLine("FROM");
            sb.Append(new string('\t', depth + 1));
            sb.Append($"{expression.Source.Translate(_dependencyContainer, depth + 1)}");

            return sb.ToString();
        }
    }
}