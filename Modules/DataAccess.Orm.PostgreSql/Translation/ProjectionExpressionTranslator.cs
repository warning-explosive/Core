namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Linq.Abstractions;
    using Linq.Expressions;
    using Linq.Internals;

    [Component(EnLifestyle.Singleton)]
    internal class ProjectionExpressionTranslator : IExpressionTranslator<ProjectionExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ProjectionExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(ProjectionExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            if (expression.IsDistinct)
            {
                sb.AppendLine("SELECT DISTINCT");
            }
            else
            {
                sb.AppendLine("SELECT");
            }

            var lastBindingIndex = expression.Bindings.Count - 1;

            if (expression.Bindings.Any())
            {
                var bindings = await expression
                    .Bindings
                    .Select(binding => binding.Translate(_dependencyContainer, depth, token))
                    .WhenAll()
                    .ConfigureAwait(false);

                bindings.Each((binding, i) =>
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
            sb.Append($"{await expression.Source.Translate(_dependencyContainer, depth + 1, token).ConfigureAwait(false)}");

            return sb.ToString();
        }
    }
}