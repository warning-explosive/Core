namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Internals;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ConditionalExpressionTranslator : IExpressionTranslator<ConditionalExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public ConditionalExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(ConditionalExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("CASE WHEN ");
            sb.Append(expression.When.Translate(_dependencyContainer, depth));
            sb.Append(" THEN ");
            sb.Append(expression.Then.Translate(_dependencyContainer, depth));
            sb.Append(" ELSE ");
            sb.Append(expression.Else.Translate(_dependencyContainer, depth));
            sb.Append(" END");

            return sb.ToString();
        }
    }
}