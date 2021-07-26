namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class NamedSourceExpressionTranslator : IExpressionTranslator<NamedSourceExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public NamedSourceExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(NamedSourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parenthesis = expression.Source is not QuerySourceExpression;

            if (parenthesis)
            {
                sb.Append("(");
            }

            sb.Append(expression.Source.Translate(_dependencyContainer, depth));

            if (parenthesis)
            {
                sb.Append(")");
            }

            sb.Append(" ");
            sb.Append(expression.Parameter.Translate(_dependencyContainer, depth));

            return sb.ToString();
        }
    }
}