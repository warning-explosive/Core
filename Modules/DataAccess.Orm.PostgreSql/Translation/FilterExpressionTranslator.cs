namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class FilterExpressionTranslator : IExpressionTranslator<FilterExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public FilterExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(FilterExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            sb.AppendLine(await expression.Source.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
            sb.Append(new string('\t', depth));
            sb.AppendLine("WHERE");
            sb.Append(new string('\t', depth + 1));
            sb.Append($"{await expression.Expression.Translate(_dependencyContainer, depth, token).ConfigureAwait(false)}");

            return sb.ToString();
        }
    }
}