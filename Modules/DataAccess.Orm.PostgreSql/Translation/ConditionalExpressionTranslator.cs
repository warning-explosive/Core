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

    [Component(EnLifestyle.Singleton)]
    internal class ConditionalExpressionTranslator : IExpressionTranslator<ConditionalExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ConditionalExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(ConditionalExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            sb.Append("CASE WHEN ");
            sb.Append(await expression.When.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
            sb.Append(" THEN ");
            sb.Append(await expression.Then.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
            sb.Append(" ELSE ");
            sb.Append(await expression.Else.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
            sb.Append(" END");

            return sb.ToString();
        }
    }
}