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
    internal class RowsFetchLimitExpressionTranslator : IExpressionTranslator<RowsFetchLimitExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public RowsFetchLimitExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(RowsFetchLimitExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(expression.Source.Translate(_dependencyContainer, depth + 1));

            sb.Append(new string('\t', depth));
            sb.Append($"fetch first {expression.RowsFetchLimit} rows only");

            return sb.ToString();
        }
    }
}