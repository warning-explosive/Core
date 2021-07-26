namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class FilterExpressionTranslator : IExpressionTranslator<FilterExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public FilterExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(FilterExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.Source.Translate(_dependencyContainer, depth));
            sb.Append(new string('\t', depth));
            sb.AppendLine("WHERE");
            sb.Append(new string('\t', depth + 1));
            sb.Append($"{expression.Expression.Translate(_dependencyContainer, depth)}");

            return sb.ToString();
        }
    }
}