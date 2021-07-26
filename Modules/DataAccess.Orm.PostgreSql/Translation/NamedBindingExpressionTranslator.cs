namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class NamedBindingExpressionTranslator : IExpressionTranslator<NamedBindingExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public NamedBindingExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(NamedBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parentheses = expression.Expression is not SimpleBindingExpression;

            if (parentheses)
            {
                sb.Append("(");
            }

            sb.Append(expression.Expression.Translate(_dependencyContainer, depth));

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