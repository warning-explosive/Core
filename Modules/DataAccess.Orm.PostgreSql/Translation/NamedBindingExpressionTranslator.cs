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
    internal class NamedBindingExpressionTranslator : IExpressionTranslator<NamedBindingExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public NamedBindingExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(NamedBindingExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            var parentheses = expression.Source is not SimpleBindingExpression;

            if (parentheses)
            {
                sb.Append("(");
            }

            sb.Append(await expression.Source.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));

            if (parentheses)
            {
                sb.Append(")");
            }

            sb.Append(" AS ");
            sb.Append(expression.Name);

            return sb.ToString();
        }
    }
}