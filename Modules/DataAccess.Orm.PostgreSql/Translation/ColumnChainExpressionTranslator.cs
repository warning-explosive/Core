namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnChainExpressionTranslator : IExpressionTranslator<ColumnChainExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ColumnChainExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(ColumnChainExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(expression.Source.Translate(_dependencyContainer, depth));
            sb.Append('.');
            sb.Append('\"');
            sb.Append(expression.Chain.Select(binding => binding.Name).ToString("_"));
            sb.Append('\"');

            return sb.ToString();
        }
    }
}