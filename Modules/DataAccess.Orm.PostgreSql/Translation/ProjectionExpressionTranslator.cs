namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Linq.Abstractions;
    using Linq.Expressions;

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

            sb.AppendLine("SELECT");

            var lastBindingIndex = expression.Bindings.Count - 1;

            if (expression.Bindings.Any())
            {
                expression
                    .Bindings
                    .Each((binding, i) =>
                    {
                        sb.Append(new string('\t', depth + 1));
                        sb.Append(binding.Translate(_dependencyContainer, depth));

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