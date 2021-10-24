namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
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

        public string Translate(NamedBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parentheses = expression.Source is not SimpleBindingExpression;

            if (parentheses)
            {
                sb.Append('(');
            }

            sb.Append(expression.Source.Translate(_dependencyContainer, depth));

            if (parentheses)
            {
                sb.Append(')');
            }

            sb.Append(" AS ");
            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}