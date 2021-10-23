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
            sb.Append($"{expression.Predicate.Translate(_dependencyContainer, depth)}");

            return sb.ToString();
        }
    }
}