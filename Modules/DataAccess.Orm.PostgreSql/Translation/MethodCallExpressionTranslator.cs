namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class MethodCallExpressionTranslator : IExpressionTranslator<MethodCallExpression>,
                                                    IResolvable<IExpressionTranslator<MethodCallExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public MethodCallExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        public string Translate(MethodCallExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (expression.Source != null)
            {
                sb.Append(expression.Source.Translate(_dependencyContainer, depth));
                sb.Append('.');
            }

            sb.Append(expression.Name);

            sb.Append('(');

            var arguments = expression
                .Arguments
                .Select(argument => argument.Translate(_dependencyContainer, depth));

            sb.Append(arguments.ToString(", "));

            sb.Append(')');

            return sb.ToString();
        }
    }
}