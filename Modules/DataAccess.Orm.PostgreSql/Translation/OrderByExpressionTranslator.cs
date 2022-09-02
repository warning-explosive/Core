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
    internal class OrderByExpressionTranslator : IExpressionTranslator<OrderByExpression>,
                                                 IResolvable<IExpressionTranslator<OrderByExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public OrderByExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(OrderByExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.Source.Translate(_dependencyContainer, depth));
            sb.AppendLine("ORDER BY");
            sb.Append(new string('\t', depth + 1));

            expression
               .Bindings
               .Select(binding => binding.Translate(_dependencyContainer, depth))
               .Each((binding, i) =>
               {
                   if (i != 0)
                   {
                       sb.Append(", ");
                   }

                   sb.Append(binding);
               });

            return sb.ToString();
        }
    }
}