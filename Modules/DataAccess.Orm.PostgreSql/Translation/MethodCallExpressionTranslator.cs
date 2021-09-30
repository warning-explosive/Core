namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class MethodCallExpressionTranslator : IExpressionTranslator<MethodCallExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public MethodCallExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        public async Task<string> Translate(MethodCallExpression expression, int depth, CancellationToken token)
        {
            var sb = new StringBuilder();

            if (expression.Source != null)
            {
                sb.Append(await expression.Source.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
                sb.Append(".");
            }

            sb.Append(expression.Name);

            sb.Append('(');

            var arguments = await expression
                .Arguments
                .Select(argument => argument.Translate(_dependencyContainer, depth, token))
                .WhenAll()
                .ConfigureAwait(false);

            sb.Append(arguments.ToString(", "));

            sb.Append(')');

            return sb.ToString();
        }
    }
}