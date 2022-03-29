namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
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

        public string Translate(SimpleBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var source = expression
               .Flatten()
               .Last()
               .Source;

            sb.Append(source.Translate(_dependencyContainer, depth));
            sb.Append('.');
            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}