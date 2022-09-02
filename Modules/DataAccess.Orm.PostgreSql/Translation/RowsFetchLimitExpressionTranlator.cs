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
    internal class RowsFetchLimitExpressionTranslator : IExpressionTranslator<RowsFetchLimitExpression>,
                                                        IResolvable<IExpressionTranslator<RowsFetchLimitExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public RowsFetchLimitExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(RowsFetchLimitExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(new string('\t', depth));
            sb.AppendLine(expression.Source.Translate(_dependencyContainer, depth));

            sb.Append(new string('\t', depth));
            sb.Append($"fetch first {expression.RowsFetchLimit} rows only");

            return sb.ToString();
        }
    }
}