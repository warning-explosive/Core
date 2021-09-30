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
    internal class SimpleBindingExpressionTranslator : IExpressionTranslator<SimpleBindingExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public SimpleBindingExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<string> Translate(SimpleBindingExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            sb.Append(await expression.Source.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}