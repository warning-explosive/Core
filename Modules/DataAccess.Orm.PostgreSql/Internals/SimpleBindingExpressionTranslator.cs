namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using System.Text;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class SimpleBindingExpressionTranslator : IExpressionTranslator<SimpleBindingExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public SimpleBindingExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(SimpleBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(expression.Expression.Translate(_dependencyContainer, depth));
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.Name);
            sb.Append('\"');

            return sb.ToString();
        }
    }
}