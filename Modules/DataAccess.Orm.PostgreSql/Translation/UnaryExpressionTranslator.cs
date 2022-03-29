namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Collections.Generic;
    using System.Text;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class UnaryExpressionTranslator : IExpressionTranslator<UnaryExpression>
    {
        private static readonly IReadOnlyDictionary<UnaryOperator, string> Operators
            = new Dictionary<UnaryOperator, string>
            {
                [UnaryOperator.Not] = "NOT"
            };

        private readonly IDependencyContainer _dependencyContainer;

        public UnaryExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(UnaryExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(Operators[expression.Operator]);
            sb.Append(" ");
            sb.Append(expression.Source.Translate(_dependencyContainer, depth));

            return sb.ToString();
        }
    }
}