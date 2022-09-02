namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class OrderByBindingExpressionTranslator : IExpressionTranslator<OrderByBindingExpression>,
                                                        IResolvable<IExpressionTranslator<OrderByBindingExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public OrderByBindingExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(OrderByBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(expression.Binding.Translate(_dependencyContainer, depth));
            sb.Append(' ');
            sb.Append(expression.OrderingDirection.ToString().ToUpperInvariant());

            return sb.ToString();
        }
    }
}