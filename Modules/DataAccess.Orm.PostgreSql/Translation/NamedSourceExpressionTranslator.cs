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
    internal class NamedSourceExpressionTranslator : IExpressionTranslator<NamedSourceExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public NamedSourceExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(NamedSourceExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            var parenthesis = expression.Source is not QuerySourceExpression;

            if (parenthesis)
            {
                sb.Append("(");
            }

            sb.Append(await expression.Source.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));

            if (parenthesis)
            {
                sb.Append(")");
            }

            sb.Append(" ");
            sb.Append(await expression.Parameter.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));

            return sb.ToString();
        }
    }
}